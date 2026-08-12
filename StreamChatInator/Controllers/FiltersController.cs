using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

                // Load detail rows for the whole batch in one query per event
                // type (plus one for the shared user-notice base rows) instead
                // of calling Find() once per candidate. The per-row Find() was
                // N+1: with a sparse filter a single page could issue up to
                // `batchSize * 2` round trips because of the separate base
                // table lookups.
                var eventDataById = LoadEventDataBatch(candidates);

                foreach (var chatEvent in candidates)
                {
                    scanCreated = chatEvent.Created;
                    scanId = chatEvent.Id;

                    if (!eventDataById.TryGetValue(chatEvent.EventId, out var eventData) || eventData == null)
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

        /// <summary>
        /// Loads the detail row for every event in <paramref name="chatEvents"/>
        /// with one query per event type (plus one for the shared user-notice
        /// base rows), keyed by <see cref="ChatEvent.EventId"/>. Returns null for
        /// events whose detail row is missing so the caller can skip them rather
        /// than crash on a single orphaned row.
        /// </summary>
        private Dictionary<string, object?> LoadEventDataBatch(IReadOnlyList<ChatEvent> chatEvents)
        {
            var eventDataById = new Dictionary<string, object?>(chatEvents.Count);
            var baseIds = new List<string>();

            foreach (var group in chatEvents.GroupBy(e => e.ChatEventType))
            {
                var ids = group.Select(e => e.EventId).Distinct().ToList();
                switch (group.Key)
                {
                    case ChatEventType.Announcement:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventAnnouncements);
                        break;
                    case ChatEventType.AnonGiftPaidUpgrade:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventAnonGiftPaidUpgrades);
                        break;
                    case ChatEventType.BitsBadgeTier:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventBitsBadgeTiers);
                        break;
                    case ChatEventType.ChatMessage:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventChatMessages);
                        break;
                    case ChatEventType.CommunityPayForward:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventCommunityPayForwards);
                        break;
                    case ChatEventType.CommunitySubscription:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventCommunitySubscriptions);
                        break;
                    case ChatEventType.ContinuedGiftedSubscription:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventContinuedGiftedSubscriptions);
                        break;
                    case ChatEventType.GiftedSubscription:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventGiftedSubscriptions);
                        break;
                    case ChatEventType.MessageCleared:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventMessageCleareds);
                        break;
                    case ChatEventType.NewSubscriber:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventNewSubscribers);
                        break;
                    case ChatEventType.PrimePaidSubscriber:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventPrimePaidSubscribers);
                        break;
                    case ChatEventType.ReSubscriber:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventReSubscribers);
                        break;
                    case ChatEventType.Ritual:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventRituals);
                        break;
                    case ChatEventType.StandardPayForward:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventStandardPayForwards);
                        break;
                    case ChatEventType.UserBanned:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventUserBanneds);
                        break;
                    case ChatEventType.UserJoined:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventUserJoineds);
                        break;
                    case ChatEventType.UserLeft:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventUserLefts);
                        break;
                    case ChatEventType.UserTimedout:
                        LoadDetails(eventDataById, baseIds, ids, _db.ChatEventUserTimedouts);
                        break;
                    default:
                        break; // event type not implemented yet - skip rather than crash
                }
            }

            if (baseIds.Count > 0)
            {
                // Merge each notice detail with its shared base row. One query
                // for every base referenced in the batch, then apply.
                var bases = _db.ChatUserNoticeBases
                    .Where(b => baseIds.Contains(b.Id))
                    .ToDictionary(b => b.Id);

                foreach (var (eventId, detail) in eventDataById.ToList())
                {
                    if (detail is ModelWithUserNoticeBase noticeDetail)
                    {
                        eventDataById[eventId] = bases.TryGetValue(noticeDetail.ChatUserNoticeBaseId, out var cunb)
                            ? Util.MergeObjects(JsonNamingPolicy.CamelCase, cunb, detail)
                            : null;
                    }
                }
            }

            return eventDataById;
        }

        private void LoadDetails<TDetail>(
            Dictionary<string, object?> eventDataById,
            List<string> baseIds,
            List<string> ids,
            DbSet<TDetail> dbSet) where TDetail : Model
        {
            var rows = dbSet.Where(r => ids.Contains(r.Id)).ToDictionary(r => r.Id);
            foreach (var id in ids)
            {
                if (rows.TryGetValue(id, out var detail))
                {
                    eventDataById[id] = detail;
                    if (detail is ModelWithUserNoticeBase noticeDetail)
                    {
                        baseIds.Add(noticeDetail.ChatUserNoticeBaseId);
                    }
                }
            }
        }

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
    }
}
