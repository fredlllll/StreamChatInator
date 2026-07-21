using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventRitual : ModelWithUserNoticeBase
    {
        /// <summary>
        /// The name of the ritual being celebrated.
        /// </summary>
        public required string MsgParamRitualName { get; set; }

        public required string Message { get; set; }

        public static ChatEventRitual FromRitual(Ritual ritual, string chatUserNoticeBaseId)
        {
            return new ChatEventRitual()
            {
                Id = GetNewId<ChatEventRitual>(),
                Message = ritual.Message,
                MsgParamRitualName = ritual.MsgParamRitualName,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
