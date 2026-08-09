using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Database;
using System.Reactive;
using System.Threading.Channels;

namespace StreamChatInator.Hubs
{
    public class ChatHub : Hub
    {
        private ChatHubData _data;
        private readonly IServiceScopeFactory _scopeFactory;
        public ChatHub(ChatHubData data, IServiceScopeFactory scopeFactory)
        {
            _data = data;
            _scopeFactory = scopeFactory;
            data.ChannelId.Subscribe(Observer.ToObserver<string>(OnChannelIdChanged));
        }

        private async void OnChannelIdChanged(Notification<string> value)
        {
            if (value.HasValue && Clients != null) //if signalr isnt initialized yet clients is null
            {
                await Clients.All.SendAsync("ChannelId", value.Value);
            }
        }

        public override async Task OnConnectedAsync()
        {
            if (_data.ChannelId.IsInitialized)
            {
                await Clients.Caller.SendAsync("ChannelId", _data.ChannelId.Value);
            }
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