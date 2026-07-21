using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventUserTimedout : Model
    {
        /// <summary>
        /// Channel that had timeout event.
        /// </summary>
        public required string Channel { get; set; }

        /// <summary>
        /// Duration of timeout
        /// </summary>
        public required TimeSpan TimeoutDuration { get; set; }

        /// <summary>
        /// Viewer that was timed out.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Id of Viewer that was timed out.
        /// </summary>
        public required string TargetUserId { get; set; }


        public static ChatEventUserTimedout FromUserTimedout(UserTimeout userTimeout)
        {
            return new ChatEventUserTimedout()
            {
                Id = GetNewId<ChatEventUserTimedout>(),
                Channel = userTimeout.Channel,
                TargetUserId = userTimeout.TargetUserId,
                TimeoutDuration = userTimeout.TimeoutDuration,
                Username = userTimeout.Username,
            };
        }
    }
}
