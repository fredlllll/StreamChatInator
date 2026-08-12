using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StreamChatInator.Database;
using StreamChatInator.Hubs;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public EventsController(DatabaseContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        /// <summary>
        /// Removes every recorded chat event so a new stream starts from a clean
        /// slate. Deletes the event index, the shared user-notice rows and every
        /// per-type detail table in one transaction, then tells all connected
        /// clients to drop their in-memory copies.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> PurgeAll()
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            var deleted = await _db.ChatEvents.ExecuteDeleteAsync();

            // Every event table follows the "ChatEvent*" naming convention, so
            // new event types are picked up automatically and non-event tables
            // (SettingValues, ...) are left alone. Two exceptions: the shared
            // user-notice table doesn't match the prefix and is event data, so
            // it's included explicitly, while ChatEventFilters matches the
            // prefix but is filter definitions (not events), so it's excluded.
            var tables = _db.Model.GetEntityTypes()
                .Select(e => e.GetTableName())
                .Where(t => t is not null
                    && (t.StartsWith("ChatEvent") || t == nameof(DatabaseContext.ChatUserNoticeBases))
                    && t != nameof(DatabaseContext.ChatEventFilters))
                .Select(t => t!);

            // All remaining event tables in one batched command (single round
            // trip) instead of one await per table. SQLite allows only one
            // writer at a time, so parallelizing the deletes would just
            // serialize them on the same connection - or throw "database is
            // locked" - without being any faster.
            await _db.Database.ExecuteSqlRawAsync(
                string.Join(";\n", tables.Select(t => $"DELETE FROM \"{t}\"")));

            await transaction.CommitAsync();

            await _hub.Clients.All.SendAsync("EventsPurged");
            return Ok(new { deleted });
        }
    }
}
