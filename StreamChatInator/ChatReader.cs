using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Hubs;
using System.Text.Json;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace StreamChatInator
{
    public class ChatReader : IAsyncDisposable
    {
        private TwitchClient _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _userName;
        private readonly ILogger<ChatReader> _logger;
        private readonly IHubContext<ChatHub> _hub;
        private readonly ChatHubData _hubData;
        private readonly TaskCompletionSource _disconnected = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private string? _currentChannelId;

        public bool IsTracking => _hubData.Tracking.IsInitialized && _hubData.Tracking.Value;

        public ChatReader(string userName, string oauthToken, ILoggerFactory loggerFactory, IHubContext<ChatHub> hub, ChatHubData hubData, IServiceScopeFactory scopeFactory)
        {
            this._userName = userName;
            this._scopeFactory = scopeFactory;
            _logger = loggerFactory.CreateLogger<ChatReader>();
            _hub = hub;
            _hubData = hubData;

            var credentials = new ConnectionCredentials(userName, oauthToken);
            _client = new TwitchClient(loggerFactory: loggerFactory);
            _client.Initialize(credentials);

            _client.OnConnected += Client_OnConnected;
            _client.OnDisconnected += Client_OnDisconnected;
            _client.OnJoinedChannel += Client_OnJoinedChannel;
            _client.OnUserStateChanged += Client_OnUserStateChanged;
            _client.OnChatCommandReceived += Client_OnChatCommandReceived;

            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnAnnouncement += Client_OnAnnouncement;
            _client.OnAnonGiftPaidUpgrade += _client_OnAnonGiftPaidUpgrade;
            _client.OnBitsBadgeTier += _client_OnBitsBadgeTier;
            _client.OnCommunityPayForward += _client_OnCommunityPayForward;
            _client.OnCommunitySubscription += _client_OnCommunitySubscription;
            _client.OnContinuedGiftedSubscription += _client_OnContinuedGiftedSubscription;
            _client.OnGiftedSubscription += _client_OnGiftedSubscription;
            _client.OnMessageCleared += _client_OnMessageCleared;
            _client.OnNewSubscriber += _client_OnNewSubscriber;
            _client.OnPrimePaidSubscriber += _client_OnPrimePaidSubscriber;
            _client.OnReSubscriber += _client_OnReSubscriber;
            _client.OnRitual += _client_OnRitual;
            _client.OnStandardPayForward += _client_OnStandardPayForward;
            _client.OnUserBanned += _client_OnUserBanned;
            _client.OnUserJoined += _client_OnUserJoined;
            _client.OnUserLeft += _client_OnUserLeft;
            _client.OnUserTimedout += _client_OnUserTimedout;
        }

        private async Task Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            PublishChannelId(e.ChatMessage.RoomId);
            await HandleEventAsync(ChatEventType.ChatMessage, () => ChatEventChatMessage.FromChatMessage(e.ChatMessage));
        }

        private async Task Client_OnAnnouncement(object? sender, OnAnnouncementArgs e)
            => await HandleUserNoticeAsync(ChatEventType.Announcement, e.Announcement, id => ChatEventAnnouncement.FromAnnouncement(e.Announcement, id));

        private async Task _client_OnAnonGiftPaidUpgrade(object? sender, OnAnonGiftPaidUpgradeArgs e)
            => await HandleUserNoticeAsync(ChatEventType.AnonGiftPaidUpgrade, e.AnonGiftPaidUpgrade, id => ChatEventAnonGiftPaidUpgrade.FromAnonGiftPaidUpgrade(e.AnonGiftPaidUpgrade, id));

        private async Task _client_OnBitsBadgeTier(object? sender, OnBitsBadgeTierArgs e)
            => await HandleUserNoticeAsync(ChatEventType.BitsBadgeTier, e.BitsBadgeTier, id => ChatEventBitsBadgeTier.FromBitsBadgeTier(e.BitsBadgeTier, id));

        private async Task _client_OnCommunityPayForward(object? sender, OnCommunityPayForwardArgs e)
            => await HandleUserNoticeAsync(ChatEventType.CommunityPayForward, e.CommunityPayForward, id => ChatEventCommunityPayForward.FromCommunityPayForward(e.CommunityPayForward, id));

        private async Task _client_OnCommunitySubscription(object? sender, OnCommunitySubscriptionArgs e)
            => await HandleUserNoticeAsync(ChatEventType.CommunitySubscription, e.GiftedSubscription, id => ChatEventCommunitySubscription.FromCommunitySubscription(e.GiftedSubscription, id));

        private async Task _client_OnContinuedGiftedSubscription(object? sender, OnContinuedGiftedSubscriptionArgs e)
            => await HandleUserNoticeAsync(ChatEventType.ContinuedGiftedSubscription, e.ContinuedGiftedSubscription, id => ChatEventContinuedGiftedSubscription.FromContinuedGiftedSubscription(e.ContinuedGiftedSubscription, id));

        private async Task _client_OnGiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
            => await HandleUserNoticeAsync(ChatEventType.GiftedSubscription, e.GiftedSubscription, id => ChatEventGiftedSubscription.FromGiftedSubscription(e.GiftedSubscription, id));

        private async Task _client_OnMessageCleared(object? sender, OnMessageClearedArgs e)
            => await HandleEventAsync(ChatEventType.MessageCleared, () => ChatEventMessageCleared.FromMessageCleared(e));

        private async Task _client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
            => await HandleUserNoticeAsync(ChatEventType.NewSubscriber, e.Subscriber, id => ChatEventNewSubscriber.FromNewSubscriber(e.Subscriber, id));

        private async Task _client_OnPrimePaidSubscriber(object? sender, OnPrimePaidSubscriberArgs e)
            => await HandleUserNoticeAsync(ChatEventType.PrimePaidSubscriber, e.PrimePaidSubscriber, id => ChatEventPrimePaidSubscriber.FromPrimePaidSubscriber(e.PrimePaidSubscriber, id));

        private async Task _client_OnReSubscriber(object? sender, OnReSubscriberArgs e)
            => await HandleUserNoticeAsync(ChatEventType.ReSubscriber, e.ReSubscriber, id => ChatEventReSubscriber.FromReSubscriber(e.ReSubscriber, id));

        private async Task _client_OnRitual(object? sender, OnRitualArgs e)
            => await HandleUserNoticeAsync(ChatEventType.Ritual, e.Ritual, id => ChatEventRitual.FromRitual(e.Ritual, id));

        private async Task _client_OnStandardPayForward(object? sender, OnStandardPayForwardArgs e)
            => await HandleUserNoticeAsync(ChatEventType.StandardPayForward, e.StandardPayForward, id => ChatEventStandardPayForward.FromStandardPayForward(e.StandardPayForward, id));

        private async Task _client_OnUserBanned(object? sender, OnUserBannedArgs e)
            => await HandleEventAsync(ChatEventType.UserBanned, () => ChatEventUserBanned.FromUserBanned(e.UserBan));

        private async Task _client_OnUserJoined(object? sender, OnUserJoinedArgs e)
            => await HandleEventAsync(ChatEventType.UserJoined, () => ChatEventUserJoined.FromUserJoined(e));

        private async Task _client_OnUserLeft(object? sender, OnUserLeftArgs e)
            => await HandleEventAsync(ChatEventType.UserLeft, () => ChatEventUserLeft.FromUserLeft(e));

        private async Task _client_OnUserTimedout(object? sender, OnUserTimedoutArgs e)
            => await HandleEventAsync(ChatEventType.UserTimedout, () => ChatEventUserTimedout.FromUserTimedout(e.UserTimeout));

        #region nonEventStuff

        private async Task HandleEventAsync<TEvent>(ChatEventType eventType, Func<TEvent> createEvent) where TEvent : Model
        {
            if (!IsTracking) return;
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            await EndEventHandler(db, eventType, createEvent());
        }

        private async Task HandleUserNoticeAsync<TDetail>(ChatEventType eventType, UserNoticeBase notice, Func<string, TDetail> createDetail) where TDetail : ModelWithUserNoticeBase
        {
            if (!IsTracking) return;
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var cunb = ChatUserNoticeBase.FromUserNoticeBase(notice);
            var detail = createDetail(cunb.Id);
            await EndEventHandler(db, eventType, detail, cunb);
        }

        private async Task EndEventHandler<T, U>(DatabaseContext db, ChatEventType chatEventType, T eventData, U eventSubData) where T : Model where U : Model
        {
            var chatEvent = new ChatEvent()
            {
                Id = Model.GetNewId<ChatEvent>(),
                ChatEventType = chatEventType,
                EventId = eventData.Id,
            };
            db.Add(eventData);
            db.Add(eventSubData);
            db.ChatEvents.Add(chatEvent);
            await db.SaveChangesAsync();

            await SendEventToFrontend(chatEvent, eventData, eventSubData);
        }

        private async Task EndEventHandler<T>(DatabaseContext db, ChatEventType chatEventType, T eventData) where T : Model
        {
            var chatEvent = new ChatEvent()
            {
                Id = Model.GetNewId<ChatEvent>(),
                ChatEventType = chatEventType,
                EventId = eventData.Id,
            };
            db.Add(eventData);
            db.ChatEvents.Add(chatEvent);
            await db.SaveChangesAsync();

            await SendEventToFrontend(chatEvent, eventData);
        }

        private async Task SendEventToFrontend(ChatEvent chatEvent, Model data)
        {
            await _hub.Clients.All.SendAsync("ReceiveEvent", Util.ToFrontendData(chatEvent, data));
        }

        private async Task SendEventToFrontend(ChatEvent chatEvent, Model data, Model subData)
        {
            await _hub.Clients.All.SendAsync("ReceiveEvent", Util.ToFrontendData(chatEvent, data, subData));
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();
        }

        async Task Client_OnConnected(object? sender, OnConnectedEventArgs e)
        {
            _logger.LogInformation("twitch client connected as " + e.BotUsername);
            await _client.JoinChannelAsync(_userName);
        }

        private async Task Client_OnUserStateChanged(object? sender, OnUserStateChangedArgs e)
        {
            // USERSTATE is sent right after joining and carries the numeric room id.
            string? roomId = null;
            if (e.UserState.UndocumentedTags is IDictionary<string, string> tags
                && tags.TryGetValue("room-id", out var value))
            {
                roomId = value;
            }
            PublishChannelId(roomId);
        }

        /// <summary>
        /// Broadcasts the numeric Twitch channel id to the frontend, but only when it changes,
        /// so we don't spam every chat message through the hub.
        /// </summary>
        private void PublishChannelId(string? channelId)
        {
            if (string.IsNullOrEmpty(channelId) || channelId == _currentChannelId) return;
            _currentChannelId = channelId;
            _hubData.ChannelId.Post(channelId);
        }

        private async Task Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            _logger.LogInformation("twitch client joined channel " + e.Channel + " as " + e.BotUsername);
        }

        async Task Client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            var command = e.Command;

            // Only the channel owner (the Twitch account that authorized the app)
            // may pause/resume tracking; otherwise anyone in chat could stop
            // recording. The bot only joins its own channel, so IsBroadcaster is
            // only ever true for the logged-in account.
            if (!e.ChatMessage.IsBroadcaster) return;

            switch (command.Name.ToLower())
            {
                case "stoptracking":
                    SetTracking(false);
                    _logger.LogInformation("tracking stopped by {User}", e.ChatMessage.Username);
                    break;
                case "starttracking":
                    SetTracking(true);
                    _logger.LogInformation("tracking started by {User}", e.ChatMessage.Username);
                    break;
            }
        }

        /// <summary>
        /// Sets whether chat events are recorded and broadcast, notifying any
        /// connected frontends so their pause/play button stays in sync. Used by
        /// both the chat commands and the SignalR RPC.
        /// </summary>
        public void SetTracking(bool enabled) => _hubData.Tracking.Post(enabled);
        public async Task Run(CancellationToken stoppingToken)
        {
            // Return when the twitch client drops so the service can reconnect,
            // or when the app is shutting down.
            var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = stoppingToken.Register(() => cancelled.TrySetResult());
            await Task.WhenAny(_disconnected.Task, cancelled.Task);
        }

        private Task Client_OnDisconnected(object? sender, OnDisconnectedArgs e)
        {
            _logger.LogInformation("twitch client disconnected");
            _disconnected.TrySetResult();
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_client.IsConnected)
                {
                    await _client.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "error disconnecting twitch client");
            }
        }
        #endregion
    }
}
