using System.Text.Json.Serialization;

namespace StreamChatInator.Database.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChatEventType
    {
        [JsonStringEnumMemberName("None")]
        None = 0,
        [JsonStringEnumMemberName("ChatMessage")]
        ChatMessage,
        [JsonStringEnumMemberName("Ban")]
        Ban,
        [JsonStringEnumMemberName("Timeout")]
        Timeout,
        [JsonStringEnumMemberName("Subscription")]
        Subscription,
    }

    public class ChatEvent : Model
    {
        public required ChatEventType ChatEventType { get; set; }
        public required string EventId { get; set; }
    }
}
