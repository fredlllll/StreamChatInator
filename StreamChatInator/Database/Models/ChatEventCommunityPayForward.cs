using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventCommunityPayForward : ModelWithUserNoticeBase
    {
        public required bool MsgParamPriorGifterAnonymous { get; set; }

        public required string MsgParamPriorGifterDisplayName { get; set; }

        public required string MsgParamPriorGifterId { get; set; }

        public required string MsgParamPriorGifterUserName { get; set; }


        public static ChatEventCommunityPayForward FromBitsBadgeTier(CommunityPayForward communityPayForward, string chatUserNoticeBaseId)
        {
            return new ChatEventCommunityPayForward()
            {
                Id = GetNewId<ChatEventCommunityPayForward>(),
                MsgParamPriorGifterAnonymous = communityPayForward.MsgParamPriorGifterAnonymous,
                MsgParamPriorGifterDisplayName = communityPayForward.MsgParamPriorGifterDisplayName,
                MsgParamPriorGifterId = communityPayForward.MsgParamPriorGifterId,
                MsgParamPriorGifterUserName = communityPayForward.MsgParamPriorGifterUserName,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
