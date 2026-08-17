using System.Text.Json.Serialization;

namespace StreamChatInator.ApiModels
{
    public class HistoryResponse
    {
        [JsonPropertyName("events")]
        public required List<FrontEndEventData> Events { get; set; }
        [JsonPropertyName("nextCursor")]
        public required string NextCursor { get; set; }
        [JsonPropertyName("hasMore")]
        public required bool HasMore { get; set; }
    }
}