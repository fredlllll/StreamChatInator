using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatUserNoticeBase : Model
    {
        public required string HexColor { get; set; }

        /// <summary>
        /// The user’s display name, escaped as described in the IRCv3 spec.
        /// </summary>
        public required string DisplayName { get; set; }

        /// <summary>
        /// List of emotes and their positions in the message.
        /// </summary>
        public required string Emotes { get; set; }

        /// <summary>
        /// An ID that uniquely identifies this message.
        /// </summary>
        public required string TwitchMessageId { get; set; }

        /// <summary>
        /// The username of the user whose action generated the message.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// The type of notice (not the ID).
        /// </summary>
        public required string MsgId { get; set; }

        /// <summary>
        /// An ID that identifies the chat room (channel).
        /// </summary>
        public required string RoomId { get; set; }

        /// <summary>
        /// The message Twitch shows in the chat room for this notice.
        /// </summary>        
        public required string SystemMsg { get; set; }

        /// <summary>
        /// The time for when the Twitch IRC server received the message.
        /// </summary>        
        public required DateTime TmiSent { get; set; }

        public required UserDetails UserFlags { get; set; }

        /// <summary>
        /// The user's chat badges as Twitch sent them in the notice's badge tag,
        /// serialized as a JSON array of {"set": "...", "version": "..."} entries.
        /// This is the ground truth for which badges to render on the client; the
        /// user flags/type below are only a fallback for badges missing from it.
        /// </summary>
        public required string? Badges { get; set; }

        public string[] UserFlagsNames => Util.FlagEnumNames(UserFlags);

        /// <summary>
        /// The user’s ID.
        /// </summary>        
        public required string UserId { get; set; }

        /// <summary>
        /// The type of user sending the whisper message.
        /// </summary>
        public required UserType UserType { get; set; }

        public string UserTypeName { get { return UserType.ToString(); } }

        public static ChatUserNoticeBase FromUserNoticeBase(UserNoticeBase unb)
        {
            var cunb = new ChatUserNoticeBase()
            {
                Id = GetNewId<ChatUserNoticeBase>(),
                DisplayName = unb.DisplayName,
                Emotes = unb.Emotes,
                HexColor = unb.HexColor,
                Username = unb.Login,
                MsgId = unb.MsgId,
                RoomId = unb.RoomId,
                SystemMsg = unb.SystemMsg,
                TmiSent = unb.TmiSent.UtcDateTime,
                TwitchMessageId = unb.Id,
                UserFlags = Util.GetPrivateFieldNotNull<UserDetails>(unb.UserDetail, "_flags"),
                UserId = unb.UserId,
                UserType = unb.UserType,
                Badges = Util.SerializeBadges(unb.Badges),
            };
            return cunb;
        }
    }
}
