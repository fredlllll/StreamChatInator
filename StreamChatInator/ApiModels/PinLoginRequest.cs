using System.Text.Json.Serialization;

namespace StreamChatInator.ApiModels
{
    public class PinLoginRequest
    {
        [JsonPropertyName("pin")]
        public required string Pin { get; set; }
    }
}