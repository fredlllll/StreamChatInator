using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Globalization;
using System.Text.Json;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiltersController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<FiltersController> _logger;
        private readonly IMemoryCache _cache;

        public FiltersController(DatabaseContext db, ILogger<FiltersController> logger, IMemoryCache cache)
        {
            _db = db;
            _logger = logger;
            _cache = cache;
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

        public class UpsertFilterRequest
        {
            public required string Name { get; set; }
            public required string Code { get; set; }
            public required string CodeJs { get; set; }
        }


        public class HistoryResponse
        {
            public required List<FrontEndEventData> Events { get; set; }
            public required string NextCursor { get; set; }
            public required bool HasMore { get; set; }
        }

        [HttpGet("{id}/messages")]
        public ActionResult<HistoryResponse> GetMessages(string id, [FromQuery] string? before, [FromQuery] int take = 50)
        {
            var filter = _db.ChatEventFilters.Find(id);
            if (filter == null) return NotFound();

            // Clamp the page size so an arbitrary query param can't ask for an
            // unbounded response (scanning is capped separately below anyway).
            take = Math.Clamp(take, 1, 100);

            var evaluator = GetEvaluator(filter);
            var (scanCreated, scanId) = ParseCursor(before);
            var matches = new List<FrontEndEventData>();
            bool exhausted = false;

            const int batchSize = 200;
            const int maxBatchesToScan = 20;

            for (int batch = 0; batch < maxBatchesToScan; batch++)
            {
                IQueryable<ChatEvent> query = _db.ChatEvents;
                if (scanCreated.HasValue)
                {
                    var created = scanCreated.Value;
                    var cursorId = scanId;
                    // Keyset: strictly before (created, id). The Id tiebreaker
                    // prevents events that share the exact same Created timestamp
                    // from being skipped across a batch boundary.
                    query = query.Where(e => e.Created < created || (e.Created == created && e.Id.CompareTo(cursorId) < 0));
                }
                var candidates = query
                    .OrderByDescending(e => e.Created)
                    .ThenByDescending(e => e.Id)
                    .Take(batchSize)
                    .ToList();

                if (candidates.Count == 0) { exhausted = true; break; }

                foreach (var chatEvent in candidates)
                {
                    scanCreated = chatEvent.Created;
                    scanId = chatEvent.Id;

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

            var nextCursor = scanCreated.HasValue ? $"{scanCreated.Value:o}|{scanId}" : "";
            return new HistoryResponse { Events = matches, NextCursor = nextCursor, HasMore = !exhausted };
        }

        /// <summary>
        /// Parses the opaque page cursor. A bare timestamp (the frontend's
        /// initial "before" value) means "start at this point"; a composite
        /// "timestamp|id" cursor is a keyset that resumes exactly where the
        /// previous page stopped.
        /// </summary>
        private static (DateTime? Created, string Id) ParseCursor(string? cursor)
        {
            if (string.IsNullOrEmpty(cursor)) return (null, "");
            var sep = cursor.LastIndexOf('|');
            // RoundtripKind keeps UTC (`...Z`) cursors in UTC; a plain parse would
            // shift them into local time and break the keyset comparison below.
            if (sep > 0 && DateTime.TryParse(cursor[..sep], null, DateTimeStyles.RoundtripKind, out var created))
            {
                return (created, cursor[(sep + 1)..]);
            }
            if (DateTime.TryParse(cursor, null, DateTimeStyles.RoundtripKind, out var plain))
            {
                return (plain, "");
            }
            return (null, "");
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
        /// <summary>
        /// Returns a reusable compiled evaluator for the filter. Compiling the
        /// Jint engine per page load is the expensive part of history queries,
        /// so cache it. Keying on <c>Updated</c> means an edit automatically
        /// compiles a fresh evaluator (the stale entry just expires).
        /// </summary>
        private JsFilterEvaluator GetEvaluator(ChatEventFilter filter)
        {
            var key = $"filter-evaluator:{filter.Id}:{filter.Updated.Ticks}";
            return _cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return new JsFilterEvaluator(filter.CodeJs);
            })!;
        }

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
