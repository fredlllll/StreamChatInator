using Microsoft.AspNetCore.SignalR;
using Open.Observable;
using StreamChatInator.Hubs;
using System.Reactive;

namespace StreamChatInator
{
    /// <summary>
    /// Holds app-wide chat state. Subscribes once to the channel id stream and
    /// broadcasts changes to all SignalR clients via <see cref="IHubContext{THub}"/> —
    /// unlike a hub instance's <c>Clients</c> property, that context stays valid for
    /// the app lifetime, so broadcasts can't hit a disposed hub.
    /// </summary>
    public class ChatHubData
    {
        public ObservableValue<string> ChannelId { get; } = new ObservableValue<string>();

        public ChatHubData(IHubContext<ChatHub> hubContext)
        {
            ChannelId.Subscribe(Observer.ToObserver<string>(value =>
            {
                if (!value.HasValue) return;
                var task = hubContext.Clients.All.SendAsync("ChannelId", value.Value);
                // fire-and-forget; a broadcast failing during shutdown is non-fatal
                _ = task.ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
            }));
        }
    }
}