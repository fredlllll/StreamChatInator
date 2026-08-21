using StreamChatInator.Database.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Adapts the recording-related events of a <see cref="TwitchClient"/>
    /// onto the app's chat event pipeline and persists them through
    /// <see cref="EventRecorder"/> while tracking is active. Holds no
    /// per-connection state apart from the last published channel id, so one
    /// instance serves every reconnect.
    /// </summary>
    public class TwitchClientEventAdapter
    {
        private readonly ChatHubData _hubData;
        private readonly EventRecorder _eventRecorder;

        public bool IsTracking => _hubData.Tracking.IsInitialized && _hubData.Tracking.Value;

        public TwitchClientEventAdapter(ChatHubData hubData, EventRecorder eventRecorder)
        {
            _hubData = hubData;
            _eventRecorder = eventRecorder;
        }

        public void Attach(TwitchClient client)
        {
            client.OnMessageReceived += OnMessageReceived;
            client.OnUserStateChanged += OnUserStateChanged;
            client.OnAnnouncement += OnAnnouncement;
            client.OnAnonGiftPaidUpgrade += OnAnonGiftPaidUpgrade;
            client.OnBitsBadgeTier += OnBitsBadgeTier;
            client.OnCommunityPayForward += OnCommunityPayForward;
            client.OnCommunitySubscription += OnCommunitySubscription;
            client.OnContinuedGiftedSubscription += OnContinuedGiftedSubscription;
            client.OnGiftedSubscription += OnGiftedSubscription;
            client.OnMessageCleared += OnMessageCleared;
            client.OnNewSubscriber += OnNewSubscriber;
            client.OnPrimePaidSubscriber += OnPrimePaidSubscriber;
            client.OnReSubscriber += OnReSubscriber;
            client.OnRitual += OnRitual;
            client.OnStandardPayForward += OnStandardPayForward;
            client.OnUserBanned += OnUserBanned;
            client.OnUserJoined += OnUserJoined;
            client.OnUserLeft += OnUserLeft;
            client.OnUserTimedout += OnUserTimedout;
        }

        private Task OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            PublishChannelId(e.ChatMessage.RoomId);
            return HandleEventAsync(ChatEventType.ChatMessage, () => ChatEventChatMessage.FromChatMessage(e.ChatMessage));
        }

        private Task OnAnnouncement(object? sender, OnAnnouncementArgs e)
            => HandleUserNoticeAsync(ChatEventType.Announcement, e.Announcement, id => ChatEventAnnouncement.FromAnnouncement(e.Announcement, id));

        private Task OnAnonGiftPaidUpgrade(object? sender, OnAnonGiftPaidUpgradeArgs e)
            => HandleUserNoticeAsync(ChatEventType.AnonGiftPaidUpgrade, e.AnonGiftPaidUpgrade, id => ChatEventAnonGiftPaidUpgrade.FromAnonGiftPaidUpgrade(e.AnonGiftPaidUpgrade, id));

        private Task OnBitsBadgeTier(object? sender, OnBitsBadgeTierArgs e)
            => HandleUserNoticeAsync(ChatEventType.BitsBadgeTier, e.BitsBadgeTier, id => ChatEventBitsBadgeTier.FromBitsBadgeTier(e.BitsBadgeTier, id));

        private Task OnCommunityPayForward(object? sender, OnCommunityPayForwardArgs e)
            => HandleUserNoticeAsync(ChatEventType.CommunityPayForward, e.CommunityPayForward, id => ChatEventCommunityPayForward.FromCommunityPayForward(e.CommunityPayForward, id));

        private Task OnCommunitySubscription(object? sender, OnCommunitySubscriptionArgs e)
            => HandleUserNoticeAsync(ChatEventType.CommunitySubscription, e.GiftedSubscription, id => ChatEventCommunitySubscription.FromCommunitySubscription(e.GiftedSubscription, id));

        private Task OnContinuedGiftedSubscription(object? sender, OnContinuedGiftedSubscriptionArgs e)
            => HandleUserNoticeAsync(ChatEventType.ContinuedGiftedSubscription, e.ContinuedGiftedSubscription, id => ChatEventContinuedGiftedSubscription.FromContinuedGiftedSubscription(e.ContinuedGiftedSubscription, id));

        private Task OnGiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
            => HandleUserNoticeAsync(ChatEventType.GiftedSubscription, e.GiftedSubscription, id => ChatEventGiftedSubscription.FromGiftedSubscription(e.GiftedSubscription, id));

        private Task OnMessageCleared(object? sender, OnMessageClearedArgs e)
            => HandleEventAsync(ChatEventType.MessageCleared, () => ChatEventMessageCleared.FromMessageCleared(e));

        private Task OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
            => HandleUserNoticeAsync(ChatEventType.NewSubscriber, e.Subscriber, id => ChatEventNewSubscriber.FromNewSubscriber(e.Subscriber, id));

        private Task OnPrimePaidSubscriber(object? sender, OnPrimePaidSubscriberArgs e)
            => HandleUserNoticeAsync(ChatEventType.PrimePaidSubscriber, e.PrimePaidSubscriber, id => ChatEventPrimePaidSubscriber.FromPrimePaidSubscriber(e.PrimePaidSubscriber, id));

        private Task OnReSubscriber(object? sender, OnReSubscriberArgs e)
            => HandleUserNoticeAsync(ChatEventType.ReSubscriber, e.ReSubscriber, id => ChatEventReSubscriber.FromReSubscriber(e.ReSubscriber, id));

        private Task OnRitual(object? sender, OnRitualArgs e)
            => HandleUserNoticeAsync(ChatEventType.Ritual, e.Ritual, id => ChatEventRitual.FromRitual(e.Ritual, id));

        private Task OnStandardPayForward(object? sender, OnStandardPayForwardArgs e)
            => HandleUserNoticeAsync(ChatEventType.StandardPayForward, e.StandardPayForward, id => ChatEventStandardPayForward.FromStandardPayForward(e.StandardPayForward, id));

        private Task OnUserBanned(object? sender, OnUserBannedArgs e)
            => HandleEventAsync(ChatEventType.UserBanned, () => ChatEventUserBanned.FromUserBanned(e.UserBan));

        private Task OnUserJoined(object? sender, OnUserJoinedArgs e)
            => HandleEventAsync(ChatEventType.UserJoined, () => ChatEventUserJoined.FromUserJoined(e));

        private Task OnUserLeft(object? sender, OnUserLeftArgs e)
            => HandleEventAsync(ChatEventType.UserLeft, () => ChatEventUserLeft.FromUserLeft(e));

        private Task OnUserTimedout(object? sender, OnUserTimedoutArgs e)
            => HandleEventAsync(ChatEventType.UserTimedout, () => ChatEventUserTimedout.FromUserTimedout(e.UserTimeout));

        private async Task HandleEventAsync<TEvent>(ChatEventType eventType, Func<TEvent> createEvent) where TEvent : Model
        {
            if (!IsTracking) return;
            await _eventRecorder.RecordAsync(eventType, createEvent());
        }

        private async Task HandleUserNoticeAsync<TDetail>(ChatEventType eventType, UserNoticeBase notice, Func<string, TDetail> createDetail) where TDetail : ModelWithUserNoticeBase
        {
            if (!IsTracking) return;
            var cunb = ChatUserNoticeBase.FromUserNoticeBase(notice);
            var detail = createDetail(cunb.Id);
            await _eventRecorder.RecordAsync(eventType, detail, cunb);
        }

        private Task OnUserStateChanged(object? sender, OnUserStateChangedArgs e)
        {
            // USERSTATE is sent right after joining and carries the numeric room id.
            string? roomId = null;
            if (e.UserState.UndocumentedTags is IDictionary<string, string> tags
                && tags.TryGetValue("room-id", out var value))
            {
                roomId = value;
            }
            PublishChannelId(roomId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Broadcasts the numeric Twitch channel id to the frontend, but only when it changes,
        /// so we don't spam every chat message through the hub.
        /// </summary>
        private void PublishChannelId(string? channelId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                return;
            }
            _hubData.ChannelId.Post(channelId, true);
        }
    }
}
