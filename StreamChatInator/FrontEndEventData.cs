using StreamChatInator.Database.Models;

namespace StreamChatInator
{
    public class FrontEndEventData
    {
        public required string EventId { get; set; }
        public required ChatEventType ChatEventType { get; set; }
        public bool Seen { get; set; }
        public required dynamic ChatEventData { get; set; }
    }
}
