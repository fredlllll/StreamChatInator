using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamChatInator.Apis
{
    public static class Twitch
    {
        public static async Task<TwitchTokenValidationResponse?> ValidateTokenAsync(string bearerToken)
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await httpClient.GetAsync("https://id.twitch.tv/oauth2/validate");

            if (response.IsSuccessStatusCode)
            {
                // Automatically parses the JSON stream into your strongly-typed class
                return await response.Content.ReadFromJsonAsync<TwitchTokenValidationResponse>();
            }

            // Handle error (e.g., return null or throw an exception)
            return null;
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
