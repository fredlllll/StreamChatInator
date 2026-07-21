using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiltersController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public FiltersController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult<List<ChatEventFilter>> GetAll()
        {
            return _db.EventFilters.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<ChatEventFilter> GetById(string id)
        {
            var filter = _db.EventFilters.Find(id);
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
            };

            _db.EventFilters.Add(filter);
            _db.SaveChanges();
            return filter;
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] UpsertFilterRequest request)
        {
            var filter = _db.EventFilters.Find(id);
            if (filter == null) return NotFound();

            filter.Name = request.Name;
            filter.Code = request.Code;
            filter.Updated = DateTime.UtcNow;

            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var filter = _db.EventFilters.Find(id);
            if (filter == null) return NotFound();

            _db.EventFilters.Remove(filter);
            _db.SaveChanges();
            return NoContent();
        }

        public class UpsertFilterRequest
        {
            public required string Name { get; set; }
            public required string Code { get; set; }
        }

        public class EventEnvelope
        {
            public required string Type { get; set; }
            public required object Data { get; set; }
        }

        public class HistoryResponse
        {
            public required List<EventEnvelope> Events { get; set; }
            public required DateTime NextCursor { get; set; }
            public required bool HasMore { get; set; }
        }

        [HttpGet("{id}/messages")]
        public ActionResult<HistoryResponse> GetMessages(string id, [FromQuery] DateTime? before, [FromQuery] int take = 50)
        {
            var filter = _db.EventFilters.Find(id);
            if (filter == null) return NotFound();

            var evaluator = new JsFilterEvaluator(filter.Code);
            var scanCursor = before ?? DateTime.UtcNow;
            var matches = new List<EventEnvelope>();
            bool exhausted = false;

            const int batchSize = 200;
            const int maxBatchesToScan = 20;

            for (int batch = 0; batch < maxBatchesToScan; batch++)
            {
                var candidates = _db.ChatEvents
                    .Where(e => e.Created < scanCursor)
                    .OrderByDescending(e => e.Created)
                    .Take(batchSize)
                    .ToList();

                if (candidates.Count == 0) { exhausted = true; break; }

                foreach (var chatEvent in candidates)
                {
                    scanCursor = chatEvent.Created;

                    var eventData = LoadEventData(chatEvent);
                    if (eventData == null) continue; // event type not implemented yet

                    var eventType = chatEvent.ChatEventType.ToString();
                    if (evaluator.Matches(eventType, eventData))
                    {
                        matches.Add(new EventEnvelope { Type = eventType, Data = eventData });
                        if (matches.Count >= take) break;
                    }
                }

                if (matches.Count >= take) break;
                if (candidates.Count < batchSize) { exhausted = true; break; }
            }

            return new HistoryResponse { Events = matches, NextCursor = scanCursor, HasMore = !exhausted };
        }

        private object? LoadEventData(ChatEvent chatEvent)
        {
            return chatEvent.ChatEventType switch
            {
                ChatEventType.ChatMessage => _db.ChatEventAnnouncements.Find(chatEvent.EventId),
                // ChatEventType.Ban => _db.ChatEventBans.Find(chatEvent.EventId),
                // ChatEventType.Timeout => _db.ChatEventTimeouts.Find(chatEvent.EventId),
                _ => null, // not implemented yet - skip rather than crash
            };
        }
    }
}