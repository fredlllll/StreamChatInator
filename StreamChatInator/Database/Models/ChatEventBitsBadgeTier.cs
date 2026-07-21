using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventBitsBadgeTier : ModelWithUserNoticeBase
    {
        /// <summary>
        /// The tier of the Bits badge the user just earned. For example, 100, 1000, or 10000.
        /// </summary>
        public required int MsgParamThreshold { get; set; }

        public static ChatEventBitsBadgeTier FromBitsBadgeTier(BitsBadgeTier bitsBadgeTier, string chatUserNoticeBaseId)
        {
            return new ChatEventBitsBadgeTier()
            {
                Id = GetNewId<ChatEventBitsBadgeTier>(),
                MsgParamThreshold = bitsBadgeTier.MsgParamThreshold,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
