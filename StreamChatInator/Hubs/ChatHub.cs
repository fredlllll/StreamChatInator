using Microsoft.AspNetCore.SignalR;

namespace StreamChatInator.Hubs
{
    public class ChatHub : Hub
    {
        public int ChannelId { get; set; }

        // future client->server RPC methods go here, e.g.:
        // public Task<int> GetActiveViewerCount() => ...

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ChannelId", ChannelId);
            await base.OnConnectedAsync();
        }
    }
}