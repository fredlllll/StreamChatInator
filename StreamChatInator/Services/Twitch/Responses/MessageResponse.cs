using System.Text.Json.Serialization;

namespace StreamChatInator.Services.Twitch.Responses
{
    public class MessageResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
