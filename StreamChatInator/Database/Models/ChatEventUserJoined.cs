using TwitchLib.Client.Events;

namespace StreamChatInator.Database.Models
{
    public class ChatEventUserJoined : Model
    {
        /// <summary>
        /// Property representing username of joined viewer.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Property representing channel bot is connected to.
        /// </summary>
        public required string Channel { get; set; }


        public static ChatEventUserJoined FromUserJoined(OnUserJoinedArgs e)
        {
            return new ChatEventUserJoined()
            {
                Id = GetNewId<ChatEventUserJoined>(),
                Channel = e.Channel,
                Username = e.Username,
            };
        }
    }
}
