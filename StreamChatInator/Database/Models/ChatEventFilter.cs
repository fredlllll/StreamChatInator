namespace StreamChatInator.Database.Models
{
    public class ChatEventFilter : Model
    {
        public required string Name { get; set; }
        public required string Code { get; set; } // TypeScript source of the filter script (defines __matches)
        public required string CodeJs { get; set; } // compiled JavaScript (full script) used for execution
    }
}
