using StreamChatInator.Services.Twitch;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Periodically asks <see cref="TwitchAuthService"/> to refresh the stored
    /// Twitch token before it expires, so consumers can read the current token
    /// from <see cref="TwitchTokenService"/> at any time without refresh logic.
    /// </summary>
    public class TwitchTokenRefreshService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly TwitchAuthService _twitchAuthService;
        private readonly ILogger<TwitchTokenRefreshService> _logger;

        public TwitchTokenRefreshService(TwitchAuthService twitchAuthService, ILogger<TwitchTokenRefreshService> logger)
        {
            _twitchAuthService = twitchAuthService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _twitchAuthService.EnsureFreshTokenAsync();
                }
                catch (Exception ex)
                {
                    // A failed refresh must never take the host down; the next
                    // tick simply tries again.
                    _logger.LogWarning(ex, "Twitch token refresh check failed");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
