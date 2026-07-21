using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventReSubscriber : ModelWithUserNoticeBase
    {
        /// <summary>
        /// The total number of months the user has subscribed.
        /// </summary>
        public required int MsgParamCumulativeMonths { get; set; }

        /// <summary>
        /// A Boolean value that indicates whether the user wants their streaks shared.
        /// </summary>
        public required bool MsgParamShouldShareStreak { get; set; }

        /// <summary>
        /// The number of consecutive months the user has subscribed.
        /// </summary>
        public required int MsgParamStreakMonths { get; set; }

        /// <summary>
        /// The type of subscription plan being used.
        /// </summary>
        public required SubscriptionPlan MsgParamSubPlan { get; set; }

        /// <summary>
        /// The display name of the subscription plan. This may be a default name or one created by the channel owner.
        /// </summary>
        public required string MsgParamSubPlanName { get; set; }

        public required string ResubMessage { get; set; }
        public static ChatEventReSubscriber FromReSubscriber(ReSubscriber reSubscriber, string chatUserNoticeBaseId)
        {
            return new ChatEventReSubscriber()
            {
                Id = GetNewId<ChatEventReSubscriber>(),
                MsgParamCumulativeMonths = reSubscriber.MsgParamCumulativeMonths,
                MsgParamShouldShareStreak = reSubscriber.MsgParamShouldShareStreak,
                MsgParamStreakMonths = reSubscriber.MsgParamStreakMonths,
                MsgParamSubPlan = reSubscriber.MsgParamSubPlan,
                MsgParamSubPlanName = reSubscriber.MsgParamSubPlanName,
                ResubMessage = reSubscriber.ResubMessage,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
