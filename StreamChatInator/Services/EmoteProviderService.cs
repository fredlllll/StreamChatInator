using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace StreamChatInator.Services
{
    public record EmoteDto(string Code, string Url);

    public class EmoteProviderService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<EmoteProviderService> _logger;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, Task<List<EmoteDto>>> _inFlight = new();

        private static readonly TimeSpan Ttl = TimeSpan.FromHours(1);

        public EmoteProviderService(IHttpClientFactory httpFactory, IMemoryCache cache, ILogger<EmoteProviderService> logger)
        {
            _httpFactory = httpFactory;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets the external emotes (BTTV/7TV/FFZ) for a channel.
        /// Use <c>null</c> for global emotes only. When a channel id is given, the
        /// result includes the global emotes merged with the channel emotes.
        /// </summary>
        public async Task<IReadOnlyList<EmoteDto>> GetEmotesAsync(string? channelId)
        {
            var global = await GetOrFetchAsync("global", FetchAllGlobalAsync);

            if (string.IsNullOrEmpty(channelId))
            {
                return global;
            }

            var channel = await GetOrFetchAsync($"channel:{channelId}", () => FetchAllChannelAsync(channelId!));

            // Channel emotes take priority over global emotes on code collision.
            var merged = new Dictionary<string, EmoteDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var emote in channel) merged[emote.Code] = emote;
            foreach (var emote in global) merged.TryAdd(emote.Code, emote);

            return merged.Values.ToList();
        }

        private async Task<List<EmoteDto>> GetOrFetchAsync(string key, Func<Task<List<EmoteDto>>> factory)
        {
            if (_cache.TryGetValue(key, out List<EmoteDto>? cached) && cached != null)
            {
                return cached;
            }

            var task = _inFlight.GetOrAdd(key, _ => FetchAndCacheAsync(key, factory));
            return await task;
        }

        private async Task<List<EmoteDto>> FetchAndCacheAsync(string key, Func<Task<List<EmoteDto>>> factory)
        {
            try
            {
                var result = await factory();
                _cache.Set(key, result, Ttl);
                return result;
            }
            finally
            {
                _inFlight.TryRemove(key, out _);
            }
        }

        private async Task<List<EmoteDto>> FetchAllGlobalAsync()
        {
            var all = await Task.WhenAll(FetchBTTVAsync(null), FetchSevenTVAsync(null), FetchFFZAsync(null));
            return all.SelectMany(x => x).ToList();
        }

        private async Task<List<EmoteDto>> FetchAllChannelAsync(string channelId)
        {
            var all = await Task.WhenAll(FetchBTTVAsync(channelId), FetchSevenTVAsync(channelId), FetchFFZAsync(channelId));
            return all.SelectMany(x => x).ToList();
        }

        private async Task<List<EmoteDto>> FetchBTTVAsync(string? channelId)
        {
            try
            {
                var client = _httpFactory.CreateClient("emotes");
                var url = channelId is null
                    ? "https://api.betterttv.net/3/cached/emotes/global"
                    : $"https://api.betterttv.net/3/cached/users/twitch/{channelId}";

                using var resp = await client.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                await using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var result = new List<EmoteDto>();
                var root = doc.RootElement;

                if (channelId is null)
                {
                    foreach (var item in root.EnumerateArray()) AddBTTVEmote(result, item);
                }
                else
                {
                    foreach (var propName in new[] { "channelEmotes", "sharedEmotes" })
                    {
                        if (root.TryGetProperty(propName, out var list))
                        {
                            foreach (var item in list.EnumerateArray()) AddBTTVEmote(result, item);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch BTTV emotes for channel {ChannelId}", channelId);
                return [];
            }
        }

        private static void AddBTTVEmote(List<EmoteDto> result, JsonElement item)
        {
            if (item.TryGetProperty("id", out var id) && item.TryGetProperty("code", out var code)
                && id.ValueKind == JsonValueKind.String && code.ValueKind == JsonValueKind.String
                && !string.IsNullOrEmpty(id.GetString()) && !string.IsNullOrEmpty(code.GetString()))
            {
                result.Add(new EmoteDto(code.GetString()!, $"https://cdn.betterttv.net/emote/{id.GetString()}/1x.webp"));
            }
        }

        private async Task<List<EmoteDto>> FetchSevenTVAsync(string? channelId)
        {
            try
            {
                var client = _httpFactory.CreateClient("emotes");
                var result = new List<EmoteDto>();
                var setUrl = channelId is null
                    ? "https://7tv.io/v3/emote-sets/global"
                    : await ResolveSevenTVSetUrlAsync(client, channelId);

                if (setUrl is null) return result;

                using var resp = await client.GetAsync(setUrl);
                resp.EnsureSuccessStatusCode();
                await using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (doc.RootElement.TryGetProperty("emotes", out var emotes))
                {
                    foreach (var item in emotes.EnumerateArray()) AddSevenTVEmote(result, item);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch 7TV emotes for channel {ChannelId}", channelId);
                return [];
            }
        }

        private static async Task<string?> ResolveSevenTVSetUrlAsync(HttpClient client, string channelId)
        {
            using var resp = await client.GetAsync($"https://7tv.io/v3/users/twitch/{channelId}");
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var root = doc.RootElement;
            if (root.TryGetProperty("emote_set", out var set)
                && set.TryGetProperty("id", out var setId) && setId.ValueKind == JsonValueKind.String
                && !string.IsNullOrEmpty(setId.GetString()))
            {
                return $"https://7tv.io/v3/emote-sets/{setId.GetString()}";
            }

            if (root.TryGetProperty("emote_set_id", out var setIdOld)
                && setIdOld.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(setIdOld.GetString()))
            {
                return $"https://7tv.io/v3/emote-sets/{setIdOld.GetString()}";
            }

            return null;
        }

        private static void AddSevenTVEmote(List<EmoteDto> result, JsonElement item)
        {
            if (item.TryGetProperty("id", out var id) && item.TryGetProperty("name", out var name)
                && id.ValueKind == JsonValueKind.String && name.ValueKind == JsonValueKind.String
                && !string.IsNullOrEmpty(id.GetString()) && !string.IsNullOrEmpty(name.GetString()))
            {
                result.Add(new EmoteDto(name.GetString()!, $"https://cdn.7tv.app/emote/{id.GetString()}/1x.webp"));
            }
        }

        private async Task<List<EmoteDto>> FetchFFZAsync(string? channelId)
        {
            try
            {
                var client = _httpFactory.CreateClient("emotes");
                var url = channelId is null
                    ? "https://api.frankerfacez.com/v1/set/3"
                    : $"https://api.frankerfacez.com/v1/room/id/{channelId}";

                using var resp = await client.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                await using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var result = new List<EmoteDto>();
                var root = doc.RootElement;

                if (channelId is null)
                {
                    if (root.TryGetProperty("set", out var set) && set.TryGetProperty("emoticons", out var emoticons))
                    {
                        foreach (var item in emoticons.EnumerateArray()) AddFFZEmote(result, item);
                    }
                }
                else if (root.TryGetProperty("sets", out var sets))
                {
                    foreach (var set in sets.EnumerateObject())
                    {
                        if (set.Value.TryGetProperty("emoticons", out var emoticons))
                        {
                            foreach (var item in emoticons.EnumerateArray()) AddFFZEmote(result, item);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch FFZ emotes for channel {ChannelId}", channelId);
                return [];
            }
        }

        private static void AddFFZEmote(List<EmoteDto> result, JsonElement item)
        {
            if (!(item.TryGetProperty("id", out var id) && item.TryGetProperty("name", out var name)
                && name.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(name.GetString())))
            {
                return;
            }

            var emoteId = id.ValueKind == JsonValueKind.String ? id.GetString() : id.GetRawText();
            if (string.IsNullOrEmpty(emoteId)) return;

            var code = name.GetString()!;

            if (item.TryGetProperty("animated", out var animated) && animated.ValueKind == JsonValueKind.Object)
            {
                if (animated.TryGetProperty("1", out var animUrl) && animUrl.ValueKind == JsonValueKind.String
                    && animUrl.GetString() is string animUrlStr)
                {
                    result.Add(new EmoteDto(code, NormalizeUrl(animUrlStr)));
                }
                else
                {
                    result.Add(new EmoteDto(code, $"https://cdn.frankerfacez.com/emote/{emoteId}/animated/1.webp"));
                }
                return;
            }

            if (item.TryGetProperty("urls", out var urls) && urls.ValueKind == JsonValueKind.Object
                && urls.TryGetProperty("1", out var url) && url.ValueKind == JsonValueKind.String
                && url.GetString() is string urlStr)
            {
                result.Add(new EmoteDto(code, NormalizeUrl(urlStr)));
                return;
            }

            result.Add(new EmoteDto(code, $"https://cdn.frankerfacez.com/emote/{emoteId}/1"));
        }

        private static string NormalizeUrl(string url)
        {
            return url.StartsWith("//", StringComparison.Ordinal) ? "https:" + url : url;
        }
    }
}