using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventPrimePaidSubscriber : ModelWithUserNoticeBase
    {
        /// <summary>
        /// The type of subscription plan being used.
        /// </summary>
        public required SubscriptionPlan MsgParamSubPlan { get; set; }

        public required string ResubMessage { get; set; }

        public static ChatEventPrimePaidSubscriber FromPrimePaidSubscriber(PrimePaidSubscriber primePaidSubscriber, string chatUserNoticeBaseId)
        {
            return new ChatEventPrimePaidSubscriber()
            {
                Id = GetNewId<ChatEventPrimePaidSubscriber>(),
                MsgParamSubPlan = primePaidSubscriber.MsgParamSubPlan,
                ResubMessage = primePaidSubscriber.ResubMessage,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
