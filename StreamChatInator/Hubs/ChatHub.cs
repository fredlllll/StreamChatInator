using Microsoft.AspNetCore.SignalR;
using System.Reactive;
using System.Threading.Channels;

namespace StreamChatInator.Hubs
{
    public class ChatHub : Hub
    {
        private ChatHubData _data;
        public ChatHub(ChatHubData data)
        {
            _data = data;
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
    }
}