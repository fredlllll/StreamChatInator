using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Text.Json;
using TwitchLib.Client;
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
        private readonly WsChannelManager _channelManager;

        public ChatReader(string userName, string oauthToken, IServiceScopeFactory scopeFactory)
        {
            this._userName = userName;
            this._scope = scopeFactory.CreateScope();
            this._scopeFactory = scopeFactory;
            var loggerFactory = _scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<ChatReader>();
            _channelManager = _scope.ServiceProvider.GetRequiredService<WsChannelManager>();

            var credentials = new ConnectionCredentials(userName, oauthToken);
            _client = new TwitchClient(loggerFactory: loggerFactory);
            _client.Initialize(credentials);
            _client.WillReplaceEmotes = true;
            _client.ReplacedEmotesPrefix = "::";
            _client.ReplacedEmotesSuffix = "]]";

            _client.OnConnected += Client_OnConnected;
            _client.OnJoinedChannel += Client_OnJoinedChannel;
            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnChatCommandReceived += Client_OnChatCommandReceived;
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

        async Task Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            _logger.LogInformation("message received: " + e.ChatMessage);
            if (tracking)
            {
                //create new scope and context every time so messages dont float around in memory for the entire runtime of the application
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var msg = e.ChatMessage;

                var chatMessage = new ChatEventMessage()
                {
                    Id = Model.GetNewId<ChatEventMessage>(),
                    Bits = msg.Bits,
                    BitsInDollars = msg.BitsInDollars,
                    CustomRewardId = msg.CustomRewardId,
                    Message = msg.Message,
                    EmoteReplacedMessage = msg.EmoteReplacedMessage,
                    IsBroadcaster = msg.IsBroadcaster,
                    IsFirstMessage = msg.IsFirstMessage,
                    IsHighlighted = msg.IsHighlighted,
                    IsMe = msg.IsMe,
                    IsSkippingSubMode = msg.IsSkippingSubMode,
                    Noisy = msg.Noisy,
                    ReplyParentMessageTwitchMessageId = msg.ChatReply?.ParentMsgId,
                    SubscribedMonthCount = msg.SubscribedMonthCount,
                    TmiSent = msg.TmiSent,
                    TwitchMessageId = msg.Id,
                };

                var chatEvent = new ChatEvent()
                {
                    Id = Model.GetNewId<ChatEvent>(),
                    ChatEventType = ChatEventType.ChatMessage,
                    EventId = chatMessage.Id,
                };

                db.ChatEventsMessages.Add(chatMessage);
                db.ChatEvents.Add(chatEvent);
                await db.SaveChangesAsync();

                var frame = new WsMessageFrame<ChatEventMessage>() { MessageType = MessageType.ChatMessage, Message = chatMessage };
                await _channelManager.Broadcast("messages", frame, CancellationToken.None);
            }
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
    }
}
