using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Database;
using System.Threading.Channels;

namespace StreamChatInator.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private ChatHubData _data;
        private readonly IServiceScopeFactory _scopeFactory;
        public ChatHub(ChatHubData data, IServiceScopeFactory scopeFactory)
        {
            _data = data;
            _scopeFactory = scopeFactory;
        }

        public override async Task OnConnectedAsync()
        {
            if (_data.ChannelId.IsInitialized)
            {
                await Clients.Caller.SendAsync("ChannelId", _data.ChannelId.Value);
            }
            // Replay the current Twitch connection state so a client that joins
            // after the last broadcast still shows the correct indicator.
            await Clients.Caller.SendAsync(_data.Connected.IsInitialized && _data.Connected.Value ? "Connection" : "NoConnection");
            await base.OnConnectedAsync();
        }

        // future client->server RPC methods go here, e.g.:
        // public Task<int> GetActiveViewerCount() => ...

        public async Task SetEventSeen(string eventId, bool seen)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var chatEvent = db.ChatEvents.Find(eventId);
            if (chatEvent == null) return;

            chatEvent.Seen = seen;
            chatEvent.Updated = DateTime.UtcNow;
            db.SaveChanges();

            await Clients.All.SendAsync("EventSeen", eventId, seen);
        }
    }
}