using TwitchLib.Client.Events;

namespace StreamChatInator.Database.Models
{
    public class ChatEventMessageCleared : Model
    {
        /// <summary>
        /// Channel that had message cleared event.
        /// </summary>
        public required string Channel { get; set; }

        /// <summary>
        /// Message contents that received clear message
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// Message ID representing the message that was cleared
        /// </summary>
        public required string TargetMessageId { get; set; }

        /// <summary>
        /// Timestamp of when message was sent
        /// </summary>
        public required DateTime TmiSent { get; set; }

        public static ChatEventMessageCleared FromMessageCleared(OnMessageClearedArgs e)
        {
            return new ChatEventMessageCleared
            {
                Id = GetNewId<ChatEventMessageCleared>(),
                Channel = e.Channel,
                Message = e.Message,
                TargetMessageId = e.TargetMessageId,
                TmiSent = e.TmiSent.UtcDateTime
            };
        }
    }
}
