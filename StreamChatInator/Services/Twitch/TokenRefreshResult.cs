namespace StreamChatInator.Services.Twitch
{
    public enum TokenRefreshStatus
    {
        Success,
        InvalidGrant,
        Failed,
    }

    /// <summary>
    /// used internally, will not be json serialized, so no explicit field names
    /// </summary>
    public class TokenRefreshResult
    {
        public TokenRefreshStatus Status { get; init; }
        public TokenResponse? Token { get; init; }
    }
}
