using System.Text.Json.Serialization;

namespace StreamChatInator.ApiModels
{
    public class UpsertFilterRequest
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("codeJs")]
        public required string CodeJs { get; set; }
    }
}