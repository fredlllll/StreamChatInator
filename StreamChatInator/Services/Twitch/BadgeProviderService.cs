using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using StreamChatInator.Services.Twitch.Settings;

namespace StreamChatInator.Services.Twitch
{
    /// <summary>
    /// Fetches Twitch chat badges (global + the connected channel's custom
    /// badges) and exposes them as a lookup keyed by badge set id and version
    /// id, e.g. [moderator][1]. Channel badges override global ones for the
    /// same set id, so a channel's custom subscriber badges win.
    /// </summary>
    public class BadgeProviderService
    {
        private readonly TwitchApiService _twitchApiService;
        private readonly ILogger<BadgeProviderService> _logger;
        private readonly IMemoryCache _cache;
        private readonly TwitchTokenSettingService _tokenService;
        private readonly ConcurrentDictionary<string, Task<Dictionary<string, Dictionary<string, BadgeDto>>>> _inFlight = new();

        private static readonly TimeSpan s_ttl = TimeSpan.FromHours(24);

        // Failures and not-yet-authenticated states are cached only briefly,
        // so a transient blip doesn't serve stale/empty badges for a full day.
        private static readonly TimeSpan s_failureTtl = TimeSpan.FromMinutes(2);

        private const string CacheKeyPrefix = "badges:";

        public BadgeProviderService(
            TwitchApiService twitchApiService,
            IMemoryCache cache,
            TwitchTokenSettingService tokenService,
            ILogger<BadgeProviderService> logger)
        {
            _twitchApiService = twitchApiService;
            _cache = cache;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Dictionary<string, Dictionary<string, BadgeDto>>> GetBadgesAsync(string? channelId)
        {
            var key = string.IsNullOrEmpty(channelId) ? CacheKeyPrefix + "global" : $"{CacheKeyPrefix}{channelId}";

            if (_cache.TryGetValue(key, out Dictionary<string, Dictionary<string, BadgeDto>>? cached) && cached != null)
            {
                return cached;
            }

            var task = _inFlight.GetOrAdd(key, _ => FetchAndCacheAsync(key, channelId));
            return await task;
        }

        private async Task<Dictionary<string, Dictionary<string, BadgeDto>>> FetchAndCacheAsync(string key, string? channelId)
        {
            try
            {
                Dictionary<string, Dictionary<string, BadgeDto>> result;
                TimeSpan ttl;

                // Null means a transient state (no credentials yet or Twitch
                // said no), not an authoritative "this channel has no badges".
                var fetched = await FetchAsync(channelId);
                if (fetched == null)
                {
                    result = [];
                    ttl = s_failureTtl;
                }
                else
                {
                    result = fetched;
                    ttl = s_ttl;
                }

                _cache.Set(key, result, ttl);
                return result;
            }
            catch (Exception ex)
            {
                // A transient Twitch API failure shouldn't 500 the badges
                // endpoint; serve nothing now, retry after the failure TTL.
                _logger.LogWarning(ex, "Failed to fetch Twitch badges for channel {ChannelId}", channelId);
                var result = new Dictionary<string, Dictionary<string, BadgeDto>>();
                _cache.Set(key, result, s_failureTtl);
                return result;
            }
            finally
            {
                _inFlight.TryRemove(key, out _);
            }
        }

        private async Task<Dictionary<string, Dictionary<string, BadgeDto>>?> FetchAsync(string? channelId)
        {
            var merged = new Dictionary<string, Dictionary<string, BadgeDto>>();

            // The background TwitchTokenRefreshService keeps the stored
            // token fresh; no refresh handling needed here.
            var token = _tokenService.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var globalBadges = await _twitchApiService.GetGlobalBadgesAsync(token);
            if (globalBadges == null)
            {
                return null;
            }
            Merge(merged, globalBadges);

            if (!string.IsNullOrEmpty(channelId))
            {
                var channelBadges = await _twitchApiService.GetChannelBadgesAsync(token, channelId);
                if (channelBadges != null)
                {
                    Merge(merged, channelBadges, overrideExisting: true);
                }
            }

            return merged;
        }

        private static void Merge(
            Dictionary<string, Dictionary<string, BadgeDto>> merged,
            IEnumerable<BadgeSet> badgeSets,
            bool overrideExisting = false)
        {
            foreach (var badgeSet in badgeSets)
            {
                if (!merged.TryGetValue(badgeSet.SetId, out var versions))
                {
                    versions = new Dictionary<string, BadgeDto>();
                    merged[badgeSet.SetId] = versions;
                }

                foreach (var version in badgeSet.Versions)
                {
                    if (overrideExisting || !versions.ContainsKey(version.Id))
                    {
                        var imageUrl = !string.IsNullOrEmpty(version.ImageUrl2x) ? version.ImageUrl2x : version.ImageUrl1x;
                        versions[version.Id] = new BadgeDto(
                            version.Title,
                            imageUrl,
                            string.IsNullOrEmpty(version.ClickAction) ? null : version.ClickAction,
                            string.IsNullOrEmpty(version.ClickUrl) ? null : version.ClickUrl);
                    }
                }
            }
        }
    }
}