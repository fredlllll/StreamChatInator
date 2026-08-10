namespace StreamChatInator.Database.Models
{
    public class ChatEventFilter : Model
    {
        public required string Name { get; set; }
        public required string Code { get; set; } // TypeScript source of the filter function
        public required string CodeJs { get; set; } // compiled JavaScript (function body) used for execution
    }
}
