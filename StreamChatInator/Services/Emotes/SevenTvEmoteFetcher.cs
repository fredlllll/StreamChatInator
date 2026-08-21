using System.Text.Json;

namespace StreamChatInator.Services.Emotes
{
    public class SevenTvEmoteFetcher : EmoteFetcherBase
    {
        private readonly ILogger<SevenTvEmoteFetcher> _logger;

        public SevenTvEmoteFetcher(IHttpClientFactory httpFactory, ILogger<SevenTvEmoteFetcher> logger)
            : base(httpFactory, logger, "7TV")
        {
            _logger = logger;
        }

        public override async Task<List<EmoteDto>> FetchAsync(string? channelId)
        {
            string url = "https://7tv.io/v3/emote-sets/global";
            if (channelId is not null)
            {
                // First resolve the user's emote set id, then fetch that set's
                // emotes. A missing/unresolvable set just means no channel emotes.
                var resolved = await ResolveSevenTVSetUrlAsync(channelId);
                if (resolved is null) return [];
                url = resolved;
            }

            return await FetchProviderAsync(url, channelId, (root, result) =>
            {
                if (root.TryGetProperty("emotes", out var emotes))
                {
                    foreach (var item in emotes.EnumerateArray()) AddNameCodeEmote(result, item, "name", "https://cdn.7tv.app/emote/{0}/1x.webp");
                }
            });
        }

        /// <summary>Resolves a channel's 7TV emote set URL. Returns null (no channel emotes) on failure.</summary>
        private async Task<string?> ResolveSevenTVSetUrlAsync(string channelId)
        {
            try
            {
                var client = HttpFactory.CreateClient(HttpClientName.Emotes.ToString());
                using var resp = await client.GetAsync($"https://7tv.io/v3/users/twitch/{channelId}");
                resp.EnsureSuccessStatusCode();
                await using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                return ResolveSevenTVSetUrl(doc.RootElement);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve 7TV emote set for channel {ChannelId}", channelId);
                return null;
            }
        }

        private static string? ResolveSevenTVSetUrl(JsonElement root)
        {
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
    }
}