using System.Text.Json;

namespace StreamChatInator.Services.Emotes
{
    public class BttvEmoteFetcher : EmoteFetcherBase
    {
        public BttvEmoteFetcher(IHttpClientFactory httpFactory, ILogger<BttvEmoteFetcher> logger)
            : base(httpFactory, logger, "BTTV")
        {
        }

        public override List<EmoteDto> ExtractFromResponse(JsonDocument response, string? channelId)
        {
            var root = response.RootElement;
            var result = new List<EmoteDto>();
            if (channelId is null)
            {
                foreach (var item in root.EnumerateArray())
                {
                    AddNameCodeEmote(result, item, "code", "https://cdn.betterttv.net/emote/{0}/1x.webp");
                }
            }
            else
            {
                foreach (var propName in new[] { "channelEmotes", "sharedEmotes" })
                {
                    if (root.TryGetProperty(propName, out var list))
                    {
                        foreach (var item in list.EnumerateArray()) AddNameCodeEmote(result, item, "code", "https://cdn.betterttv.net/emote/{0}/1x.webp");
                    }
                }
            }
            return result;
        }

        public override async Task<List<EmoteDto>> FetchAsync(string? channelId)
        {
            var url = channelId is null
                ? "https://api.betterttv.net/3/cached/emotes/global"
                : $"https://api.betterttv.net/3/cached/users/twitch/{channelId}";

            return await FetchFromUrlAsync(url, channelId);
        }
    }
}