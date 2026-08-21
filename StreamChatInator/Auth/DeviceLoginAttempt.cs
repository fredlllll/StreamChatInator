using StreamChatInator.Services.Twitch;

namespace StreamChatInator.Auth
{
    /// <summary>
    /// An in-flight device-code login attempt, keyed by the polling id handed
    /// back to the frontend. Once Twitch grants tokens they are cached here,
    /// so a retried poll validates them instead of re-polling the single-use
    /// device code.
    /// </summary>
    public class DeviceLoginAttempt
    {
        public required string DeviceCode { get; init; }
        public DateTime ExpiresAt { get; init; }
        public TokenResponse? IssuedToken { get; init; }
    }
}
