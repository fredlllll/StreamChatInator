using Microsoft.Extensions.Logging;
using StreamChatInator.Services.Twitch.Settings;
using System.Globalization;

namespace StreamChatInator.Services.Twitch
{
    /// <summary>
    /// Owns the Twitch credential lifecycle: login signaling, persisting fresh
    /// credentials, and keeping the stored token refreshed (via
    /// <see cref="TwitchTokenRefreshService"/>). Consumers read the current
    /// token from <see cref="TwitchTokenSettingService"/> - they never need this class.
    /// </summary>
    public class TwitchAuthService
    {
        private readonly TwitchOAuthService _twitchOAuthService;
        private readonly TwitchTokenSettingService _twitchTokenSettingService;
        private readonly TwitchRefreshTokenSettingService _twitchRefreshTokenSettingService;
        private readonly TwitchTokenExpiresAtSettingService _twitchTokenExpiresAtSettingService;
        private readonly ILogger<TwitchAuthService> _logger;

        public TwitchAuthService(TwitchOAuthService twitchOAuthService, TwitchTokenSettingService twitchTokenSettingService, TwitchRefreshTokenSettingService twitchRefreshTokenSettingService, TwitchTokenExpiresAtSettingService twitchTokenExpiresAtSettingService, ILogger<TwitchAuthService> logger)
        {
            _twitchOAuthService = twitchOAuthService;
            _twitchTokenSettingService = twitchTokenSettingService;
            _twitchRefreshTokenSettingService = twitchRefreshTokenSettingService;
            _twitchTokenExpiresAtSettingService = twitchTokenExpiresAtSettingService;
            _logger = logger;
        }

        /// <summary>
        /// Refreshes the stored token when it is inside the expiry window.
        /// Called periodically by <see cref="TwitchTokenRefreshService"/> so
        /// consumers reading the token from <see cref="TwitchTokenSettingService"/>
        /// always get a usable one without refresh logic of their own. No-op
        /// when no credentials exist or the token is still fresh.
        /// </summary>
        public async Task EnsureFreshTokenAsync()
        {
            var token = _twitchTokenSettingService.GetToken();
            if (string.IsNullOrEmpty(token) || !NeedsRefresh())
            {
                return;
            }

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            var refreshToken = _twitchRefreshTokenSettingService.GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return;
            }

            TokenRefreshResult result;
            try
            {
                result = await _twitchOAuthService.RefreshAccessTokenAsync(refreshToken);
            }
            catch (Exception ex)
            {
                // Network hiccups etc. are transient; the next tick retries.
                _logger.LogWarning(ex, "twitch token refresh failed");
                return;
            }

            if (result.Status == TokenRefreshStatus.InvalidGrant)
            {
                // The refresh token is dead (revoked or superseded by a newer
                // login). Clear everything so the app shows its logged-out
                // state instead of retrying a doomed refresh every minute.
                _logger.LogWarning("twitch refresh token was rejected; clearing stored credentials until re-login");
                ClearCredentials();
                return;
            }

            if (result.Status != TokenRefreshStatus.Success || result.Token == null || string.IsNullOrEmpty(result.Token.AccessToken))
            {
                // Keep the old credentials; a transient Twitch failure must
                // not log the user out.
                _logger.LogWarning("twitch token refresh failed; keeping existing credentials");
                return;
            }

            var refreshed = result.Token;

            // Persist the rotation details first, then publish the new token
            // through the token service last, so watchers never see a new
            // token alongside a stale expiry.
            if (!string.IsNullOrEmpty(refreshed.RefreshToken))
            {
                _twitchRefreshTokenSettingService.SetRefreshToken(refreshed.RefreshToken);
            }
            _twitchTokenExpiresAtSettingService.SetTokenExpiresAt(DateTime.UtcNow.AddSeconds(refreshed.ExpiresIn).ToString("o"));
            _twitchTokenSettingService.SetToken(refreshed.AccessToken);
        }

        private void ClearCredentials()
        {
            _twitchRefreshTokenSettingService.UnsetRefreshToken();
            _twitchTokenExpiresAtSettingService.UnsetTokenExpiresAt();
            _twitchTokenSettingService.UnsetToken();
        }

        private bool NeedsRefresh()
        {
            var expiresRaw = _twitchTokenExpiresAtSettingService.GetTokenExpiresAt();
            if (string.IsNullOrEmpty(expiresRaw))
            {
                return true;
            }
            if (!DateTime.TryParse(expiresRaw, null, DateTimeStyles.RoundtripKind, out var expires))
            {
                return true;
            }
            return expires <= DateTime.UtcNow.AddMinutes(5);
        }
    }
}