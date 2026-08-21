using System.Text.Json.Serialization;

namespace StreamChatInator.Services.Twitch.Responses
{
    public class MessageResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        // RFC-6749 style error bodies use "error", some Twitch endpoints put
        // the reason in "message"; both are checked by callers.
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
