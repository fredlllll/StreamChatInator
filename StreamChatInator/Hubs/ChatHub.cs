using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Database;

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
            // Replay the current tracking state for the same reason.
            await Clients.Caller.SendAsync("TrackingState", _data.Tracking.IsInitialized && _data.Tracking.Value);
            await base.OnConnectedAsync();
        }

        // future client->server RPC methods go here, e.g.:
        // public Task<int> GetActiveViewerCount() => ...

        /// <summary>
        /// Pauses/resumes recording chat events. Internally this just updates the
        /// shared tracking flag, which the <see cref="ChatReader"/> checks before
        /// saving each event, and broadcasts the new state to every client.
        /// </summary>
        public void SetTracking(bool enabled)
        {
            if (_data.Tracking.IsInitialized && _data.Tracking.Value == enabled) return;
            _data.Tracking.Post(enabled);
        }

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