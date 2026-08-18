using Microsoft.Extensions.Caching.Memory;
using StreamChatInator.ApiModels;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Globalization;
using System.Text.Json;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Runs the keyset-paged, filter-evaluated history query for a chat filter.
    /// Owns the per-type DbSet lookup and the compiled-filter cache so the
    /// controller stays a thin pass-through.
    /// </summary>
    public class EventHistoryService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventHistoryService> _logger;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Maps each chat event type to the DbSet that holds its detail rows. Used
        /// to load a batch one query per type without a per-type switch.
        /// </summary>
        private static readonly Dictionary<ChatEventType, Func<DatabaseContext, IQueryable<Model>>> EventSets = new()
        {
            [ChatEventType.Announcement] = db => db.ChatEventAnnouncements,
            [ChatEventType.AnonGiftPaidUpgrade] = db => db.ChatEventAnonGiftPaidUpgrades,
            [ChatEventType.BitsBadgeTier] = db => db.ChatEventBitsBadgeTiers,
            [ChatEventType.ChatMessage] = db => db.ChatEventChatMessages,
            [ChatEventType.CommunityPayForward] = db => db.ChatEventCommunityPayForwards,
            [ChatEventType.CommunitySubscription] = db => db.ChatEventCommunitySubscriptions,
            [ChatEventType.ContinuedGiftedSubscription] = db => db.ChatEventContinuedGiftedSubscriptions,
            [ChatEventType.GiftedSubscription] = db => db.ChatEventGiftedSubscriptions,
            [ChatEventType.MessageCleared] = db => db.ChatEventMessageCleareds,
            [ChatEventType.NewSubscriber] = db => db.ChatEventNewSubscribers,
            [ChatEventType.PrimePaidSubscriber] = db => db.ChatEventPrimePaidSubscribers,
            [ChatEventType.ReSubscriber] = db => db.ChatEventReSubscribers,
            [ChatEventType.Ritual] = db => db.ChatEventRituals,
            [ChatEventType.StandardPayForward] = db => db.ChatEventStandardPayForwards,
            [ChatEventType.UserBanned] = db => db.ChatEventUserBanneds,
            [ChatEventType.UserJoined] = db => db.ChatEventUserJoineds,
            [ChatEventType.UserLeft] = db => db.ChatEventUserLefts,
            [ChatEventType.UserTimedout] = db => db.ChatEventUserTimedouts,
        };

        public EventHistoryService(IServiceScopeFactory scopeFactory, ILogger<EventHistoryService> logger, IMemoryCache cache)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Loads a page of chat events that match the filter. Returns null when
        /// the filter doesn't exist (the caller maps that to NotFound).
        /// </summary>
        public HistoryResponse? GetMessages(string filterId, string? before, int take)
        {
            var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var filter = db.ChatEventFilters.Find(filterId);
            if (filter == null) return null;

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
                IQueryable<ChatEvent> query = db.ChatEvents;
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
            var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var eventDataById = new Dictionary<string, object?>(chatEvents.Count);
            var baseIds = new List<string>();

            foreach (var group in chatEvents.GroupBy(e => e.ChatEventType))
            {
                var ids = group.Select(e => e.EventId).Distinct().ToList();
                // Only the event types actually present in this batch are queried, so
                // adding a new event type means adding one entry to the map (not a
                // new case in a switch). Unimplemented types are skipped entirely.
                if (EventSets.TryGetValue(group.Key, out var getSet))
                {
                    LoadDetails(eventDataById, baseIds, ids, getSet(db));
                }
            }

            if (baseIds.Count > 0)
            {
                // Merge each notice detail with its shared base row. One query
                // for every base referenced in the batch, then apply.
                var bases = db.ChatUserNoticeBases
                    .Where(b => baseIds.Contains(b.Id))
                    .ToDictionary(b => b.Id);

                foreach (var (eventId, detail) in eventDataById.ToList())
                {
                    if (detail is ModelWithUserNoticeBase noticeDetail)
                    {
                        eventDataById[eventId] = bases.TryGetValue(noticeDetail.ChatUserNoticeBaseId, out var cunb)
                            ? ObjectMerger.MergeObjects(JsonNamingPolicy.CamelCase, cunb, detail)
                            : null;
                    }
                }
            }

            return eventDataById;
        }

        private static void LoadDetails(
            Dictionary<string, object?> eventDataById,
            List<string> baseIds,
            List<string> ids,
            IQueryable<Model> dbSet)
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