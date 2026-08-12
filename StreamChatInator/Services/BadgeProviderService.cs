using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using StreamChatInator.Apis;

namespace StreamChatInator.Services
{
    public record BadgeDto(string Title, string ImageUrl, string? ClickAction, string? ClickUrl);

    /// <summary>
    /// Fetches Twitch chat badges (global + the connected channel's custom
    /// badges) and exposes them as a lookup keyed by badge set id and version
    /// id, e.g. [moderator][1]. Channel badges override global ones for the
    /// same set id, so a channel's custom subscriber badges win.
    /// </summary>
    public class BadgeProviderService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<BadgeProviderService> _logger;
        private readonly IMemoryCache _cache;
        private readonly TwitchTokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly ConcurrentDictionary<string, Task<Dictionary<string, Dictionary<string, BadgeDto>>>> _inFlight = new();

        private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

        public BadgeProviderService(
            IHttpClientFactory httpFactory,
            IMemoryCache cache,
            TwitchTokenService tokenService,
            IConfiguration config,
            ILogger<BadgeProviderService> logger)
        {
            _httpFactory = httpFactory;
            _cache = cache;
            _tokenService = tokenService;
            _config = config;
            _logger = logger;
        }

        private string ClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

        public async Task<Dictionary<string, Dictionary<string, BadgeDto>>> GetBadgesAsync(string? channelId)
        {
            var key = string.IsNullOrEmpty(channelId) ? "badges:global" : $"badges:{channelId}";

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
                var result = await FetchAsync(channelId);
                _cache.Set(key, result, Ttl);
                return result;
            }
            finally
            {
                _inFlight.TryRemove(key, out _);
            }
        }

        private async Task<Dictionary<string, Dictionary<string, BadgeDto>>> FetchAsync(string? channelId)
        {
            var merged = new Dictionary<string, Dictionary<string, BadgeDto>>();
            // Set once per fetch when the stored token proves stale/revoked, so
            // the global and channel steps each get one refresh-and-retry but we
            // never refresh more than once in a single request.
            var tokenRefreshed = false;

            try
            {
                var token = await _tokenService.GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return merged;
                }

                var http = _httpFactory.CreateClient("twitch");

                var globalBadges = await Twitch.GetGlobalBadgesAsync(http, ClientId, token);
                if (globalBadges == null)
                {
                    // The stored token may have been revoked outside its expiry
                    // window; force a refresh and retry once before giving up.
                    var refreshed = await RefreshTokenOnceAsync(tokenRefreshed);
                    if (refreshed == null)
                    {
                        return merged;
                    }
                    tokenRefreshed = true;
                    token = refreshed;
                    globalBadges = await Twitch.GetGlobalBadgesAsync(http, ClientId, token);
                    if (globalBadges == null)
                    {
                        return merged;
                    }
                }
                Merge(merged, globalBadges);

                if (!string.IsNullOrEmpty(channelId))
                {
                    var channelBadges = await Twitch.GetChannelBadgesAsync(http, ClientId, token, channelId);
                    if (channelBadges == null && !tokenRefreshed)
                    {
                        // The global call returned before noticing a stale token
                        // (e.g. only the channel-scoped endpoint 401s), so retry
                        // the channel fetch with a freshly refreshed token too.
                        var refreshed = await RefreshTokenOnceAsync(tokenRefreshed);
                        if (refreshed != null)
                        {
                            tokenRefreshed = true;
                            token = refreshed;
                            channelBadges = await Twitch.GetChannelBadgesAsync(http, ClientId, token, channelId);
                        }
                    }
                    if (channelBadges != null)
                    {
                        Merge(merged, channelBadges, overrideExisting: true);
                    }
                }
            }
            catch (Exception ex)
            {
                // A transient Twitch API failure shouldn't 500 the badges
                // endpoint; return whatever we managed to fetch (or nothing).
                _logger.LogWarning(ex, "Failed to fetch Twitch badges for channel {ChannelId}", channelId);
            }

            return merged;
        }

        /// <summary>
        /// Refreshes the Twitch access token unless this request already did. The
        /// <paramref name="alreadyRefreshed"/> flag is honored so a broken token can
        /// only cause a single refresh per fetch; returns null when no usable token
        /// can be obtained.
        /// </summary>
        private async Task<string?> RefreshTokenOnceAsync(bool alreadyRefreshed)
        {
            if (alreadyRefreshed)
            {
                return null;
            }
            var token = await _tokenService.RefreshAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Could not obtain a valid token to fetch Twitch badges");
                return null;
            }
            return token;
        }

        private static void Merge(
            Dictionary<string, Dictionary<string, BadgeDto>> merged,
            IEnumerable<Twitch.TwitchBadgeSet> badgeSets,
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