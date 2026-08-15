using Microsoft.AspNetCore.Mvc;
using StreamChatInator.ApiModels;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Services;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiltersController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private readonly EventHistoryService _history;

        public FiltersController(DatabaseContext db, EventHistoryService history)
        {
            _db = db;
            _history = history;
        }

        [HttpGet]
        public ActionResult<List<ChatEventFilter>> GetAll()
        {
            return _db.ChatEventFilters.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<ChatEventFilter> GetById(string id)
        {
            var filter = _db.ChatEventFilters.Find(id);
            if (filter == null) return NotFound();
            return filter;
        }

        [HttpPost]
        public ActionResult<ChatEventFilter> Create([FromBody] UpsertFilterRequest request)
        {
            var filter = new ChatEventFilter
            {
                Id = Model.GetNewId<ChatEventFilter>(),
                Name = request.Name,
                Code = request.Code,
                CodeJs = request.CodeJs,
            };

            _db.ChatEventFilters.Add(filter);
            _db.SaveChanges();
            return filter;
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] UpsertFilterRequest request)
        {
            var filter = _db.ChatEventFilters.Find(id);
            if (filter == null) return NotFound();

            filter.Name = request.Name;
            filter.Code = request.Code;
            filter.CodeJs = request.CodeJs;
            filter.Updated = DateTime.UtcNow;

            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var filter = _db.ChatEventFilters.Find(id);
            if (filter == null) return NotFound();

            _db.ChatEventFilters.Remove(filter);
            _db.SaveChanges();
            return NoContent();
        }

        [HttpGet("{id}/messages")]
        public ActionResult<HistoryResponse> GetMessages(string id, [FromQuery] string? before, [FromQuery] int take = 50)
        {
            var response = _history.GetMessages(id, before, take);
            if (response == null) return NotFound();
            return response;
        }
    }
}