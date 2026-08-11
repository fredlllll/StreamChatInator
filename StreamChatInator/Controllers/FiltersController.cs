using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Text.Json;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiltersController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<FiltersController> _logger;

        public FiltersController(DatabaseContext db, ILogger<FiltersController> logger)
        {
            _db = db;
            _logger = logger;
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
                CodeJs = request.CodeJs,
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
            filter.CodeJs = request.CodeJs;
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
            public required string CodeJs { get; set; }
        }


        public class HistoryResponse
        {
            public required List<FrontEndEventData> Events { get; set; }
            public required DateTime NextCursor { get; set; }
            public required bool HasMore { get; set; }
        }

        [HttpGet("{id}/messages")]
        public ActionResult<HistoryResponse> GetMessages(string id, [FromQuery] DateTime? before, [FromQuery] int take = 50)
        {
            var filter = _db.EventFilters.Find(id);
            if (filter == null) return NotFound();

            var evaluator = new JsFilterEvaluator(filter.CodeJs);
            var scanCursor = before ?? DateTime.UtcNow;
            var matches = new List<FrontEndEventData>();
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
                    if (eventData == null)
                    {
                        _logger.LogWarning("event data missing for {Type} event {EventId}, skipping", chatEvent.ChatEventType, chatEvent.Id);
                        continue;
                    }

                    var frontendData = new FrontEndEventData() { EventId = chatEvent.Id, ChatEventType = chatEvent.ChatEventType, Seen = chatEvent.Seen, ChatEventData = eventData };
                    if (evaluator.Matches(frontendData))
                    {
                        matches.Add(frontendData);
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
            var db = _db;
            return chatEvent.ChatEventType switch
            {
                ChatEventType.Announcement => FindWithChatUserNoticeBase(db.ChatEventAnnouncements.Find(chatEvent.EventId)),
                ChatEventType.AnonGiftPaidUpgrade => FindWithChatUserNoticeBase(db.ChatEventAnonGiftPaidUpgrades.Find(chatEvent.EventId)),
                ChatEventType.BitsBadgeTier => FindWithChatUserNoticeBase(db.ChatEventBitsBadgeTiers.Find(chatEvent.EventId)),
                ChatEventType.ChatMessage => db.ChatEventChatMessages.Find(chatEvent.EventId),
                ChatEventType.CommunityPayForward => FindWithChatUserNoticeBase(db.ChatEventCommunityPayForwards.Find(chatEvent.EventId)),
                ChatEventType.CommunitySubscription => FindWithChatUserNoticeBase(db.ChatEventCommunitySubscriptions.Find(chatEvent.EventId)),
                ChatEventType.ContinuedGiftedSubscription => FindWithChatUserNoticeBase(db.ChatEventContinuedGiftedSubscriptions.Find(chatEvent.EventId)),
                ChatEventType.GiftedSubscription => FindWithChatUserNoticeBase(db.ChatEventGiftedSubscriptions.Find(chatEvent.EventId)),
                ChatEventType.MessageCleared => db.ChatEventMessageCleareds.Find(chatEvent.EventId),
                ChatEventType.NewSubscriber => FindWithChatUserNoticeBase(db.ChatEventNewSubscribers.Find(chatEvent.EventId)),
                ChatEventType.PrimePaidSubscriber => FindWithChatUserNoticeBase(db.ChatEventPrimePaidSubscribers.Find(chatEvent.EventId)),
                ChatEventType.ReSubscriber => FindWithChatUserNoticeBase(db.ChatEventReSubscribers.Find(chatEvent.EventId)),
                ChatEventType.Ritual => FindWithChatUserNoticeBase(db.ChatEventRituals.Find(chatEvent.EventId)),
                ChatEventType.StandardPayForward => FindWithChatUserNoticeBase(db.ChatEventStandardPayForwards.Find(chatEvent.EventId)),
                ChatEventType.UserBanned => db.ChatEventUserBanneds.Find(chatEvent.EventId),
                ChatEventType.UserJoined => db.ChatEventUserJoineds.Find(chatEvent.EventId),
                ChatEventType.UserLeft => db.ChatEventUserLefts.Find(chatEvent.EventId),
                ChatEventType.UserTimedout => db.ChatEventUserTimedouts.Find(chatEvent.EventId),
                _ => null, // event type not implemented yet - skip rather than crash
            };
        }

        /// <summary>
        /// Loads the event's detail row merged with its shared ChatUserNoticeBase.
        /// Returns null (instead of throwing) when either row is missing, so a
        /// single orphaned event can't take down the whole history request.
        /// </summary>
        private object? FindWithChatUserNoticeBase(ModelWithUserNoticeBase? mwunb)
        {
            if (mwunb == null)
            {
                return null;
            }
            var cunb = _db.ChatUserNoticeBases.Find(mwunb.ChatUserNoticeBaseId);
            if (cunb == null)
            {
                return null;
            }
            return Util.MergeObjects(JsonNamingPolicy.CamelCase, cunb, mwunb);
        }
    }
}