using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamChatInator.Apis
{
    public static class Twitch
    {
        private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
        private const string ValidateUrl = "https://id.twitch.tv/oauth2/validate";
        private const string GlobalBadgesUrl = "https://api.twitch.tv/helix/chat/badges/global";
        private const string ChannelBadgesUrl = "https://api.twitch.tv/helix/chat/badges";

        public static async Task<TwitchTokenResponse?> ExchangeCodeAsync(HttpClient httpClient, string clientId, string code, string codeVerifier, string redirectUri)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["code"] = code,
                ["code_verifier"] = codeVerifier,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirectUri,
            });

            using var response = await httpClient.PostAsync(TokenUrl, form);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TwitchTokenResponse>();
        }

        public static async Task<TwitchTokenResponse?> RefreshAccessTokenAsync(HttpClient httpClient, string clientId, string refreshToken)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
            });

            using var response = await httpClient.PostAsync(TokenUrl, form);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TwitchTokenResponse>();
        }

        public static async Task<TwitchTokenValidationResponse?> ValidateTokenAsync(HttpClient httpClient, string bearerToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, ValidateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            using var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TwitchTokenValidationResponse>();
        }

        public static async Task<List<TwitchBadgeSet>?> GetGlobalBadgesAsync(HttpClient httpClient, string clientId, string bearerToken)
        {
            return await GetBadgesAsync(httpClient, clientId, bearerToken, GlobalBadgesUrl);
        }

        public static async Task<List<TwitchBadgeSet>?> GetChannelBadgesAsync(HttpClient httpClient, string clientId, string bearerToken, string broadcasterId)
        {
            var url = $"{ChannelBadgesUrl}?broadcaster_id={Uri.EscapeDataString(broadcasterId)}";
            return await GetBadgesAsync(httpClient, clientId, bearerToken, url);
        }

        private static async Task<List<TwitchBadgeSet>?> GetBadgesAsync(HttpClient httpClient, string clientId, string bearerToken, string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Headers.Add("Client-Id", clientId);

            using var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var body = await response.Content.ReadFromJsonAsync<TwitchBadgesResponse>();
            return body?.Data;
        }

        public class TwitchBadgesResponse
        {
            [JsonPropertyName("data")]
            public List<TwitchBadgeSet> Data { get; set; } = new();
        }

        public class TwitchBadgeSet
        {
            [JsonPropertyName("set_id")]
            public string SetId { get; set; } = string.Empty;

            [JsonPropertyName("versions")]
            public List<TwitchBadgeVersion> Versions { get; set; } = new();
        }

        public class TwitchBadgeVersion
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("image_url_1x")]
            public string ImageUrl1x { get; set; } = string.Empty;

            [JsonPropertyName("image_url_2x")]
            public string ImageUrl2x { get; set; } = string.Empty;

            [JsonPropertyName("image_url_4x")]
            public string ImageUrl4x { get; set; } = string.Empty;

            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }

        public class TwitchTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("scope")]
            public List<string> Scope { get; set; } = new();

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;
        }

        public class TwitchTokenValidationResponse
        {
            [JsonPropertyName("client_id")]
            public string ClientId { get; set; } = string.Empty;

            [JsonPropertyName("login")]
            public string Login { get; set; } = string.Empty;

            [JsonPropertyName("scopes")]
            public List<string> Scopes { get; set; } = new();

            [JsonPropertyName("user_id")]
            public string UserId { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}