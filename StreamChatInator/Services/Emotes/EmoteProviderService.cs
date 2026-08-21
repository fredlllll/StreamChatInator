using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace StreamChatInator.Services.Emotes
{
    /// <summary>
    /// Gets the external emotes (BTTV/7TV/FFZ) for a channel, delegating the
    /// per-provider wire format to the registered <see cref="IEmoteFetcher"/>s
    /// and owning the cache + in-flight dedup. Use <c>null</c> for global emotes
    /// only. When a channel id is given, the result includes the global emotes
    /// merged with the channel emotes.
    /// </summary>
    public class EmoteProviderService
    {
        private readonly IEnumerable<IEmoteFetcher> _fetchers;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, Task<List<EmoteDto>>> _inFlight = new();

        private static readonly TimeSpan s_ttl = TimeSpan.FromHours(1);

        public EmoteProviderService(IEnumerable<IEmoteFetcher> fetchers, IMemoryCache cache)
        {
            _fetchers = fetchers;
            _cache = cache;
        }

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
                _cache.Set(key, result, s_ttl);
                return result;
            }
            finally
            {
                _inFlight.TryRemove(key, out _);
            }
        }

        private async Task<List<EmoteDto>> FetchAllGlobalAsync()
        {
            return await FetchAllAsync(null);
        }

        private async Task<List<EmoteDto>> FetchAllChannelAsync(string channelId)
        {
            return await FetchAllAsync(channelId);
        }

        private async Task<List<EmoteDto>> FetchAllAsync(string? channelId)
        {
            var all = await Task.WhenAll(_fetchers.Select(f => f.FetchAsync(channelId)));
            return all.SelectMany(x => x).ToList();
        }
    }
}