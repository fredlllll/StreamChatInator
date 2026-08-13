using System.Text.Json.Serialization;

namespace StreamChatInator.Services.Twitch
{
    public class BadgeSet
    {
        [JsonPropertyName("set_id")]
        public string SetId { get; set; } = string.Empty;

        [JsonPropertyName("versions")]
        public List<BadgeVersion> Versions { get; set; } = new();
    }
}
