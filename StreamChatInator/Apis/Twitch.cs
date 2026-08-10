using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamChatInator.Apis
{
    public static class Twitch
    {
        private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
        private const string ValidateUrl = "https://id.twitch.tv/oauth2/validate";

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