using System.Text.Json.Serialization;

namespace StreamChatInator.Database.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChatEventType
    {
        [JsonStringEnumMemberName(nameof(None))]
        None = 0,
        [JsonStringEnumMemberName(nameof(Announcement))]
        Announcement,
        [JsonStringEnumMemberName(nameof(AnonGiftPaidUpgrade))]
        AnonGiftPaidUpgrade,
        [JsonStringEnumMemberName(nameof(BitsBadgeTier))]
        BitsBadgeTier,
        [JsonStringEnumMemberName(nameof(ChatMessage))]
        ChatMessage,
        [JsonStringEnumMemberName(nameof(CommunityPayForward))]
        CommunityPayForward,
        [JsonStringEnumMemberName(nameof(CommunitySubscription))]
        CommunitySubscription,
        [JsonStringEnumMemberName(nameof(ContinuedGiftedSubscription))]
        ContinuedGiftedSubscription,
        [JsonStringEnumMemberName(nameof(GiftedSubscription))]
        GiftedSubscription,
        [JsonStringEnumMemberName(nameof(MessageCleared))]
        MessageCleared,
        [JsonStringEnumMemberName(nameof(NewSubscriber))]
        NewSubscriber,
        [JsonStringEnumMemberName(nameof(PrimePaidSubscriber))]
        PrimePaidSubscriber,
        [JsonStringEnumMemberName(nameof(ReSubscriber))]
        ReSubscriber,
        [JsonStringEnumMemberName(nameof(Ritual))]
        Ritual,
        [JsonStringEnumMemberName(nameof(StandardPayForward))]
        StandardPayForward,
        [JsonStringEnumMemberName(nameof(UserBanned))]
        UserBanned,
        [JsonStringEnumMemberName(nameof(UserJoined))]
        UserJoined,
        [JsonStringEnumMemberName(nameof(UserLeft))]
        UserLeft,
        [JsonStringEnumMemberName(nameof(UserTimedout))]
        UserTimedout,
    }

    public class ChatEvent : Model
    {
        public required ChatEventType ChatEventType { get; set; }
        public required string EventId { get; set; }
        public bool Seen { get; set; }
    }
}
