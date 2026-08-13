using System.Text.Json.Serialization;

namespace StreamChatInator.Services.Twitch
{
    public class BadgesResponse
    {
        [JsonPropertyName("data")]
        public List<BadgeSet> Data { get; set; } = new();
    }
}
