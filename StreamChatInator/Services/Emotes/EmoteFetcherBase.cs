using System.Text.Json;

namespace StreamChatInator.Services.Emotes
{
    /// <summary>
    /// Shared scaffolding for the emote providers: create the "emotes" HTTP
    /// client, GET <paramref name="url"/>, parse the JSON response, and hand the
    /// root element to <paramref name="extract"/> to fill the result list. Any
    /// failure logs a warning and yields an empty list rather than propagating
    /// (an emote provider going down must not break chat).
    /// </summary>
    public abstract class EmoteFetcherBase : IEmoteFetcher
    {
        private readonly ILogger _logger;
        protected readonly IHttpClientFactory HttpFactory;
        private readonly string _provider;

        protected EmoteFetcherBase(IHttpClientFactory httpFactory, ILogger logger, string provider)
        {
            HttpFactory = httpFactory;
            _logger = logger;
            _provider = provider;
        }

        public abstract Task<List<EmoteDto>> FetchAsync(string? channelId);

        protected async Task<List<EmoteDto>> FetchProviderAsync(string url, string? channelId, Action<JsonElement, List<EmoteDto>> extract)
        {
            try
            {
                var client = HttpFactory.CreateClient("emotes");
                using var resp = await client.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                await using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var result = new List<EmoteDto>();
                extract(doc.RootElement, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch {Provider} emotes for channel {ChannelId}", _provider, channelId);
                return [];
            }
        }

        /// <summary>Adds an emote whose url is derived from a string "id" property and whose code comes from a named property.</summary>
        protected static void AddNameCodeEmote(List<EmoteDto> result, JsonElement item, string codeProperty, string urlTemplate)
        {
            if (item.TryGetProperty("id", out var id) && item.TryGetProperty(codeProperty, out var code)
                && id.ValueKind == JsonValueKind.String && code.ValueKind == JsonValueKind.String
                && !string.IsNullOrEmpty(id.GetString()) && !string.IsNullOrEmpty(code.GetString()))
            {
                result.Add(new EmoteDto(code.GetString()!, string.Format(urlTemplate, id.GetString())));
            }
        }
    }
}