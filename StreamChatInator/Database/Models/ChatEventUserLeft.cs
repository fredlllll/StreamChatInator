using TwitchLib.Client.Events;

namespace StreamChatInator.Database.Models
{
    public class ChatEventUserLeft : Model
    {
        /// <summary>
        /// Property representing username of user that left.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Property representing channel bot is connected to.
        /// </summary>
        public required string Channel { get; set; }

        public static ChatEventUserLeft FromUserLeft(OnUserLeftArgs e)
        {
            return new ChatEventUserLeft()
            {
                Id = GetNewId<ChatEventUserLeft>(),
                Channel = e.Channel,
                Username = e.Username,
            };
        }
    }
}
