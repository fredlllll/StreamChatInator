using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace StreamChatInator
{
    public class WsChannelManager
    {
        private sealed class ChannelContext
        {
            public readonly ConcurrentBag<WebSocket> WebSockets = new ConcurrentBag<WebSocket>();
        }

        private sealed class SubscriptionContext
        {
            public required WebSocket WebSocket;
            public required string SubscribedChannel;
        }

        private readonly ConcurrentDictionary<string, ChannelContext> _channels = new();
        private readonly ConcurrentDictionary<Guid, SubscriptionContext> subscriptions = new();
        private readonly ILogger<WsChannelManager> _logger;

        public WsChannelManager(ILogger<WsChannelManager> logger)
        {
            _logger = logger;
        }

        private ChannelContext? GetChannel(string name)
        {
            if (_channels.TryGetValue(name, out ChannelContext? context))
            {
                return context;
            }

            return null;
        }

        private ChannelContext GetChannelOrCreate(string name)
        {
            lock (_channels)
            {
                if (_channels.TryGetValue(name, out ChannelContext? context))
                {
                    return context;
                }
                return _channels[name] = new ChannelContext();
            }
        }

        public Guid Subscribe(WebSocket webSocket, string channelName)
        {
            var sub = new SubscriptionContext() { WebSocket = webSocket,SubscribedChannel = channelName };
            var id = Guid.NewGuid();
            subscriptions[id] = sub;
            var channel = GetChannelOrCreate(channelName);
            lock (channel.WebSockets)
            {
                channel.WebSockets.Add(webSocket);
            }
            return id;
        }

        public async Task Broadcast<T>(string channelName, WsMessageFrame<T> frame, CancellationToken ct)
        {
            var channel = GetChannel(channelName);
            if(channel != null)
            {
                var data = JsonSerializer.Serialize(frame);
                var bytes = Encoding.UTF8.GetBytes(data);

                IEnumerable<Task> broadcastTasks;
                lock (channel.WebSockets)
                {
                    broadcastTasks = channel.WebSockets.Where(ws => ws.State == WebSocketState.Open).Select(async ws =>
                    {
                        try
                        {
                            await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
                        }
                        catch (WebSocketException ex)
                        {
                            _logger.LogWarning("websocket is dead: " + ex.Message);
                            //TODO: handle deletion of subscription
                        }
                    });
                }
                await Task.WhenAll(broadcastTasks);
            }
        }
    }
}
