using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventCommunitySubscription : ModelWithUserNoticeBase
    {
        public required bool IsAnonymous { get; set; }

        //public required Goal? MsgParamGoal { get; set; }

        public required string MsgParamGiftTheme { get; set; }

        public required int MsgParamMassGiftCount { get; set; }

        public required string MsgParamOriginId { get; set; }

        public required int MsgParamSenderCount { get; set; }

        /// <summary>
        /// The type of subscription plan being used.
        /// </summary>
        public required SubscriptionPlan MsgParamSubPlan { get; set; }

        public string MsgParamSubPlanName => MsgParamSubPlan.ToString();

        public static ChatEventCommunitySubscription FromCommunitySubscription(CommunitySubscription giftedSubscription, string chatUserNoticeBaseId)
        {
            return new ChatEventCommunitySubscription()
            {
                Id = GetNewId<ChatEventCommunitySubscription>(),
                IsAnonymous = giftedSubscription.IsAnonymous,
                MsgParamGiftTheme = giftedSubscription.MsgParamGiftTheme,
                MsgParamMassGiftCount = giftedSubscription.MsgParamMassGiftCount,
                MsgParamOriginId = giftedSubscription.MsgParamOriginId,
                MsgParamSenderCount = giftedSubscription.MsgParamSenderCount,
                MsgParamSubPlan = giftedSubscription.MsgParamSubPlan,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
