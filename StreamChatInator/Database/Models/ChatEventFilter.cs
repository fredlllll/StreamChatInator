namespace StreamChatInator.Database.Models
{
    public class ChatEventFilter : Model
    {
        public required string Name { get; set; }
        public required string Code { get; set; } // body of a function(event) that returns true/false
    }
}
