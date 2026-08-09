using Open.Observable;

namespace StreamChatInator
{
    public class ChatHubData
    {
        public ObservableValue<string> ChannelId { get; } = new ObservableValue<string>();
    }
}
