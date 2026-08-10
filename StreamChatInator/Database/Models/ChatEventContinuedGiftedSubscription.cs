using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventContinuedGiftedSubscription : ModelWithUserNoticeBase
    {
        /// <summary>
        /// The number of gifts the gifter has given during the promo indicated by TwitchLib.Client.Models.ContinuedGiftedSubscription.MsgParamPromoName.
        /// </summary>
        public required int MsgParamPromoGiftTotal { get; set; }

        /// <summary>
        /// The subscriptions promo, if any, that is ongoing (for example, Subtember 2018).
        /// </summary>
        public required string MsgParamPromoName { get; set; }

        /// <summary>
        /// The username of the user who gifted the subscription.
        /// </summary>
        public required string MsgParamSenderUsername { get; set; }

        /// <summary>
        /// The display name of the user who gifted the subscription.
        /// </summary>
        public required string MsgParamSenderName { get; set; }

        public static ChatEventContinuedGiftedSubscription FromContinuedGiftedSubscription(ContinuedGiftedSubscription continuedGiftedSubscription, string chatUserNoticeBaseId)
        {
            return new ChatEventContinuedGiftedSubscription()
            {
                Id = GetNewId<ChatEventContinuedGiftedSubscription>(),
                MsgParamPromoGiftTotal = continuedGiftedSubscription.MsgParamPromoGiftTotal,
                MsgParamPromoName = continuedGiftedSubscription.MsgParamPromoName,
                MsgParamSenderUsername = continuedGiftedSubscription.MsgParamSenderLogin,
                MsgParamSenderName = continuedGiftedSubscription.MsgParamSenderName,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
