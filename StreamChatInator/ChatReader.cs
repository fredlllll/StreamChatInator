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
    public class ChatReader
    {
        private bool tracking = true;
        private TwitchClient _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _userName;
        private readonly IServiceScope _scope;
        private readonly ILogger<ChatReader> _logger;
        private readonly IHubContext<ChatHub> _hub;

        public ChatReader(string userName, string oauthToken, IServiceScopeFactory scopeFactory)
        {
            this._userName = userName;
            this._scope = scopeFactory.CreateScope();
            this._scopeFactory = scopeFactory;
            var loggerFactory = _scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<ChatReader>();
            _hub = _scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

            var credentials = new ConnectionCredentials(userName, oauthToken);
            _client = new TwitchClient(loggerFactory: loggerFactory);
            _client.Initialize(credentials);
            _client.WillReplaceEmotes = true;
            _client.ReplacedEmotesPrefix = "[[";
            _client.ReplacedEmotesSuffix = "]]";

            _client.OnConnected += Client_OnConnected;
            _client.OnJoinedChannel += Client_OnJoinedChannel;
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
            if (tracking)
            {
                //create new scope and context every time so messages dont float around in memory for the entire runtime of the application
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var chatMessage = ChatEventChatMessage.FromChatMessage(e.ChatMessage);

                await EndEventHandler(db, chatMessage, ChatEventType.ChatMessage);
            }
        }

        private async Task Client_OnAnnouncement(object? sender, OnAnnouncementArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.Announcement);
                var eventData = ChatEventAnnouncement.FromAnnouncement(e.Announcement, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnAnonGiftPaidUpgrade(object? sender, OnAnonGiftPaidUpgradeArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.AnonGiftPaidUpgrade);
                var eventData = ChatEventAnonGiftPaidUpgrade.FromAnonGiftPaidUpgrade(e.AnonGiftPaidUpgrade, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnBitsBadgeTier(object? sender, OnBitsBadgeTierArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.BitsBadgeTier);
                var eventData = ChatEventBitsBadgeTier.FromBitsBadgeTier(e.BitsBadgeTier, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnCommunityPayForward(object? sender, OnCommunityPayForwardArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.CommunityPayForward);
                var eventData = ChatEventCommunityPayForward.FromBitsBadgeTier(e.CommunityPayForward, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnCommunitySubscription(object? sender, OnCommunitySubscriptionArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.GiftedSubscription);
                var eventData = ChatEventCommunitySubscription.FromCommunitySubscription(e.GiftedSubscription, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnContinuedGiftedSubscription(object? sender, OnContinuedGiftedSubscriptionArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.ContinuedGiftedSubscription);
                var eventData = ChatEventContinuedGiftedSubscription.FromContinuedGiftedSubscription(e.ContinuedGiftedSubscription, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnGiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.GiftedSubscription);
                var eventData = ChatEventGiftedSubscription.FromGiftedSubscription(e.GiftedSubscription, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.Subscriber);
                var eventData = ChatEventNewSubscriber.FromNewSubscriber(e.Subscriber, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnMessageCleared(object? sender, OnMessageClearedArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var eventData = ChatEventMessageCleared.FromMessageCleared(e);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnPrimePaidSubscriber(object? sender, OnPrimePaidSubscriberArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.PrimePaidSubscriber);
                var eventData = ChatEventPrimePaidSubscriber.FromPrimePaidSubscriber(e.PrimePaidSubscriber, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnReSubscriber(object? sender, OnReSubscriberArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.ReSubscriber);
                var eventData = ChatEventReSubscriber.FromReSubscriber(e.ReSubscriber, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnRitual(object? sender, OnRitualArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.Ritual);
                var eventData = ChatEventRitual.FromRitual(e.Ritual, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnStandardPayForward(object? sender, OnStandardPayForwardArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var cunb = ChatUserNoticeBase.FromUserNoticeBase(e.StandardPayForward);
                var eventData = ChatEventStandardPayForward.FromStandardPayForward(e.StandardPayForward, cunb.Id);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnUserBanned(object? sender, OnUserBannedArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var eventData = ChatEventUserBanned.FromUserBanned(e.UserBan);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnUserJoined(object? sender, OnUserJoinedArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var eventData = ChatEventUserJoined.FromUserJoined(e);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnUserLeft(object? sender, OnUserLeftArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var eventData = ChatEventUserLeft.FromUserLeft(e);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        private async Task _client_OnUserTimedout(object? sender, OnUserTimedoutArgs e)
        {
            if (tracking)
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var eventData = ChatEventUserTimedout.FromUserTimedout(e.UserTimeout);

                //TODO: now event data doesnt contain all the stuff from cunb, we need to merge this before sending it out
                await EndEventHandler(db, eventData, ChatEventType.Announcement);
            }
        }

        
        #region nonEventStuff

        private async Task EndEventHandler<T>(DatabaseContext db, T eventData, ChatEventType chatEventType) where T : Model
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

            await _hub.Clients.All.SendAsync("ReceiveEvent", new
            {
                Type = chatEvent.ChatEventType.ToString(),
                Data = eventData,
            });
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

        private async Task Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            _logger.LogInformation("twitch client joined channel " + e.Channel + " as " + e.BotUsername);
        }



        async Task Client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            var channel = e.ChatMessage.Channel;
            var command = e.Command;

            switch (command.Name.ToLower())
            {
                case "stoptracking":
                    tracking = false;
                    break;
                case "starttracking":
                    tracking = true;
                    break;
            }
        }
        public async Task Run(CancellationToken stoppingToken)
        {
            //in theory we should wait here till disconnect or something idk
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(int.MaxValue, stoppingToken);
            }
        }
        #endregion
    }
}
