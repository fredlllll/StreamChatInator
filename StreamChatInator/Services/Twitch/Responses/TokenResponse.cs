using System.Text.Json.Serialization;

namespace StreamChatInator.Services.Twitch
{
    public class TokenResponse
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
}
