using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace StreamChatInator
{
    public class WebsocketManager
    {
        private sealed class ConnectionContext
        {
            public required WebSocket WebSocket;
            public ConcurrentBag<string> subscribedChannels = new();
        }

        private readonly ConcurrentDictionary<Guid, ConnectionContext> _connections = new();
        private readonly WsChannelManager _channelManager;

        public WebsocketManager(WsChannelManager channelManager)
        {
            this._channelManager = channelManager;
        }

        private async Task<(WebSocketReceiveResult result,string messageText)> ReceiveMessage(WebSocket ws, CancellationToken ct)
        {
            var buffer = new byte[1024 * 4];
            using var memoryStream = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                memoryStream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);
            return (result, Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        private void HandleMessageFrame(Guid connectionId, WsMessageFrame<JsonElement> frame)
        {
            var context = _connections[connectionId];
            switch (frame.MessageType)
            {
                case MessageType.SubscribeChannel:
                    var msg = frame.Message.Deserialize<WsMessageSubscribeChannel>();
                    if(msg == null)
                    {
                        throw new InvalidDataException("could not deserialize message");
                    }
                    context.subscribedChannels.Add(msg.Channel);
                    _channelManager.Subscribe(context.WebSocket, msg.Channel);
                    break;
            }
        }

        public async Task SubscribeAsync(Guid connectionId, WebSocket webSocket, CancellationToken ct)
        {
            var connection = new ConnectionContext() { WebSocket = webSocket };
            _connections[connectionId] = connection;            
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var msg = await ReceiveMessage(webSocket, ct);

                    switch (msg.result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            var frame = JsonSerializer.Deserialize<WsMessageFrame<JsonElement>>(msg.messageText);
                            if(frame == null)
                            {
                                throw new Exception("could not deserialize json: " + msg.messageText);
                            }
                            HandleMessageFrame(connectionId, frame);
                            break;
                        case WebSocketMessageType.Close:
                            return;
                    }
                }
            }
            finally
            {
                await UnsubscribeAsync(connectionId);
            }
        }

        private async Task UnsubscribeAsync(Guid connectionId)
        {
            await Util.RetryAsync(() => _connections.TryRemove(connectionId, out _), 500);
        }
    }
}
