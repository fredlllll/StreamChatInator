namespace StreamChatInator.ApiModels
{
    public class HistoryResponse
    {
        public required List<FrontEndEventData> Events { get; set; }
        public required string NextCursor { get; set; }
        public required bool HasMore { get; set; }
    }
}