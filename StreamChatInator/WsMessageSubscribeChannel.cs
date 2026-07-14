using System.Text.Json.Serialization;

namespace StreamChatInator
{
    public class WsMessageSubscribeChannel
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; } = string.Empty;
    }
}
