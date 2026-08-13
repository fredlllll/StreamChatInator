using static StreamChatInator.Services.Twitch.TwitchApiService;

namespace StreamChatInator.Services.Twitch
{
    public enum DevicePollStatus
    {
        Pending,
        Success,
        Failed,
    }

    public class DevicePollResult
    {
        public DevicePollStatus Status { get; init; }
        public TokenResponse? Token { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
