using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventAnonGiftPaidUpgrade : ModelWithUserNoticeBase
    {
        /// <summary>
        /// The number of gifts the gifter has given during the promo indicated by TwitchLib.Client.Models.AnonGiftPaidUpgrade.MsgParamPromoName.
        /// </summary>
        public required int MsgParamPromoGiftTotal { get; set; }

        /// <summary>
        /// The subscriptions promo, if any, that is ongoing (for example, Subtember 2018).
        /// </summary>
        public required string MsgParamPromoName { get; set; }

        public static ChatEventAnonGiftPaidUpgrade FromAnonGiftPaidUpgrade(AnonGiftPaidUpgrade anonGiftPaidUpgrade, string chatUserNoticeBaseId)
        {
            return new ChatEventAnonGiftPaidUpgrade()
            {
                Id = GetNewId<ChatEventAnonGiftPaidUpgrade>(),
                MsgParamPromoGiftTotal = anonGiftPaidUpgrade.MsgParamPromoGiftTotal,
                MsgParamPromoName = anonGiftPaidUpgrade.MsgParamPromoName,
                ChatUserNoticeBaseId = chatUserNoticeBaseId
            };
        }
    }
}
