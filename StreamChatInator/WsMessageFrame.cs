using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamChatInator
{
    public class WsMessageFrame<T>
    {
        [JsonPropertyName("messageType")]
        public MessageType MessageType { get; set; }

        [JsonPropertyName("message")]
        public T Message { get; set; } = default!;
    }

    [JsonConverter(typeof(JsonStringEnumConverter<MessageType>))]
    public enum MessageType
    {
        [JsonStringEnumMemberName("none")]
        None=0,
        [JsonStringEnumMemberName("subscribe_channel")]
        SubscribeChannel,
        [JsonStringEnumMemberName("chat_message")]
        ChatMessage,
    }
}
