using System.Text.Json;

namespace StreamChatInator.Services.Emotes
{
    public class FfzEmoteFetcher : EmoteFetcherBase
    {
        public FfzEmoteFetcher(IHttpClientFactory httpFactory, ILogger<FfzEmoteFetcher> logger)
            : base(httpFactory, logger, "FFZ")
        {
        }

        public override List<EmoteDto> ExtractFromResponse(JsonDocument response, string? channelId)
        {
            var root = response.RootElement;
            var result = new List<EmoteDto>();

            if (channelId is null)
            {
                if (root.TryGetProperty("set", out var set) && set.TryGetProperty("emoticons", out var emoticons))
                {
                    foreach (var item in emoticons.EnumerateArray()) AddFfzEmote(result, item);
                }
            }
            else if (root.TryGetProperty("sets", out var sets))
            {
                foreach (var set in sets.EnumerateObject())
                {
                    if (set.Value.TryGetProperty("emoticons", out var emoticons))
                    {
                        foreach (var item in emoticons.EnumerateArray()) AddFfzEmote(result, item);
                    }
                }
            }

            return result;
        }

        public override async Task<List<EmoteDto>> FetchAsync(string? channelId)
        {
            var url = channelId is null
                ? "https://api.frankerfacez.com/v1/set/3"
                : $"https://api.frankerfacez.com/v1/room/id/{channelId}";

            return await FetchFromUrlAsync(url, channelId);
        }

        private static void AddFfzEmote(List<EmoteDto> result, JsonElement item)
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