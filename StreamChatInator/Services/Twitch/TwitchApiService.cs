using StreamChatInator.Services.Twitch.Responses;
using System.Net.Http.Headers;

namespace StreamChatInator.Services.Twitch
{
    /// <summary>
    /// Helix API calls (api.twitch.tv). OAuth endpoints live in
    /// <see cref="TwitchOAuthClient"/>.
    /// </summary>
    public class TwitchApiService
    {
        private const string GlobalBadgesUrl = "https://api.twitch.tv/helix/chat/badges/global";
        private const string ChannelBadgesUrl = "https://api.twitch.tv/helix/chat/badges";

        private readonly ConfigService _config;
        private readonly HttpClient _httpClient;

        public TwitchApiService(HttpClient httpClient, ConfigService config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<BadgeSet>?> GetGlobalBadgesAsync(string bearerToken)
        {
            return await GetBadgesAsync(bearerToken, GlobalBadgesUrl);
        }

        public async Task<List<BadgeSet>?> GetChannelBadgesAsync(string bearerToken, string broadcasterId)
        {
            var url = $"{ChannelBadgesUrl}?broadcaster_id={Uri.EscapeDataString(broadcasterId)}";
            return await GetBadgesAsync(bearerToken, url);
        }

        private async Task<List<BadgeSet>?> GetBadgesAsync(string bearerToken, string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Headers.Add("Client-Id", _config.TwitchClientId);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var body = await response.Content.ReadFromJsonAsync<BadgesResponse>();
            return body?.Data;
        }
    }
}
