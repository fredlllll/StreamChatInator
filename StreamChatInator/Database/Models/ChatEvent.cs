namespace StreamChatInator.Database.Models
{
    public enum ChatEventType
    {
        None = 0,
        ChatMessage
    }

    public class ChatEvent :Model
    {
        public required ChatEventType ChatEventType { get; set; }
        public required string EventId { get; set; }
    }
}
