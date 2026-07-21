using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventGiftedSubscription : ModelWithUserNoticeBase
    {
        public required bool IsAnonymous { get; set; }

        /// <summary>
        /// The total number of months the user has subscribed.
        /// </summary>
        public required string MsgParamMonths { get; set; }

        /// <summary>
        /// If this message sourced from an event, this is the ID of that event
        /// </summary>
        public required string MsgParamOriginId { get;  set; }

        /// <summary>
        /// The display name of the subscription gift recipient.
        /// </summary>
        public required string MsgParamRecipientDisplayName { get; set; }

        /// <summary>
        /// The user ID of the subscription gift recipient.
        /// </summary>
        public required string MsgParamRecipientId { get; set; }

        /// <summary>
        /// The user name of the subscription gift recipient.
        /// </summary>
        public required string MsgParamRecipientUserName { get; set; }

        public required int MsgParamSenderCount { get; set; }

        /// <summary>
        /// The type of subscription plan being used.
        /// </summary>
        public required SubscriptionPlan MsgParamSubPlan { get; set; }

        /// <summary>
        /// The display name of the subscription plan. This may be a default name or one created by the channel owner.
        /// </summary>
        public required string MsgParamSubPlanName { get; set; }

        /// <summary>
        /// The number of months gifted as part of a single, multi-month gift.
        /// </summary>
        public required int MsgParamMultiMonthGiftDuration { get; set; }

        public static ChatEventGiftedSubscription FromGiftedSubscription(GiftedSubscription giftedSubscription, string chatUserNoticeBaseId)
        {
            return new ChatEventGiftedSubscription()
            {
                Id = GetNewId<ChatEventGiftedSubscription>(),
                IsAnonymous = giftedSubscription.IsAnonymous,
                MsgParamMonths = giftedSubscription.MsgParamMonths,
                MsgParamOriginId = giftedSubscription.MsgParamOriginId,
                MsgParamRecipientDisplayName = giftedSubscription.MsgParamRecipientDisplayName,
                MsgParamRecipientId = giftedSubscription.MsgParamRecipientId,
                MsgParamRecipientUserName = giftedSubscription.MsgParamRecipientUserName,
                MsgParamSenderCount = giftedSubscription.MsgParamSenderCount,
                MsgParamSubPlan = giftedSubscription.MsgParamSubPlan,
                MsgParamSubPlanName = giftedSubscription.MsgParamSubPlanName,
                MsgParamMultiMonthGiftDuration = giftedSubscription.MsgParamMultiMonthGiftDuration,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
