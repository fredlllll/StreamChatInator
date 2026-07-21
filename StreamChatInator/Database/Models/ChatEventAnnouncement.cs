using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventAnnouncement : ModelWithUserNoticeBase
    {
        /// <summary>
        /// Property representing the color value of the announcement.
        /// </summary>
        public required string MsgParamColor { get;  set; }

        /// <summary>
        /// Property representing the message of the announcement.
        /// </summary>
        public required string Message { get; set; }


        public static ChatEventAnnouncement FromAnnouncement(Announcement ann, string chatUserNoticeBaseId)
        {
            var eventData = new ChatEventAnnouncement()
            {
                Id = GetNewId<ChatEventAnnouncement>(),
                Message = ann.Message,
                MsgParamColor = ann.MsgParamColor,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
            return eventData;
        }
    }
}
