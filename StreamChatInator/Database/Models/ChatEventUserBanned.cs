using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventUserBanned : Model
    {
        /// <summary>
        /// Channel that had ban event.
        /// </summary>
        public required string Channel { get; set; }

        /// <summary>
        /// User that was banned.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// room that had ban event. Id.
        /// </summary>
        public required string RoomId { get; set; }

        /// <summary>
        /// User that was banned. Id.
        /// </summary>
        public required string TargetUserId { get; set; }

        public static ChatEventUserBanned FromUserBanned(UserBan userBan)
        {
            return new ChatEventUserBanned()
            {
                Id = GetNewId<ChatEventUserBanned>(),
                Channel = userBan.Channel,
                RoomId = userBan.RoomId,
                TargetUserId = userBan.TargetUserId,
                Username = userBan.Username,
            };
        }
    }
}
