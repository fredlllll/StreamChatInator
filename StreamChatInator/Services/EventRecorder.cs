using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Hubs;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Persists a chat event (index row + per-type detail rows) and broadcasts it
    /// to every SignalR client. Shared by <see cref="ChatReader"/> (real Twitch
    /// traffic) and the test-data endpoint so synthetic events go through exactly
    /// the same save + publish path as real ones.
    /// </summary>
    public class EventRecorder
    {
        private readonly IHubContext<ChatHub> _hub;

        public EventRecorder(IHubContext<ChatHub> hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// Saves a single event. <paramref name="eventData"/> is the per-type
        /// detail row; <paramref name="subData"/> is an optional shared row (the
        /// ChatUserNoticeBase for user-notice events) merged into the payload.
        /// </summary>
        public async Task RecordAsync(DatabaseContext db, ChatEventType chatEventType, Model eventData, Model? subData = null)
        {
            var chatEvent = new ChatEvent()
            {
                Id = Model.GetNewId<ChatEvent>(),
                ChatEventType = chatEventType,
                EventId = eventData.Id,
            };
            db.Add(eventData);
            if (subData != null)
            {
                db.Add(subData);
            }
            db.ChatEvents.Add(chatEvent);
            await db.SaveChangesAsync();

            var frontendData = subData == null
                ? Util.ToFrontendData(chatEvent, eventData)
                : Util.ToFrontendData(chatEvent, eventData, subData);
            await _hub.Clients.All.SendAsync("ReceiveEvent", frontendData);
        }
    }
}
