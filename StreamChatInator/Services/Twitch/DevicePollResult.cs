namespace StreamChatInator.Services.Twitch
{
    public enum DevicePollStatus
    {
        Pending,
        Success,
        Failed,
    }

    /// <summary>
    /// used internally, will not be json serialized, so no explicit field names
    /// </summary>
    public class DevicePollResult
    {
        public DevicePollStatus Status { get; init; }
        public TokenResponse? Token { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
