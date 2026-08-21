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

        public TwitchAuthService(TwitchOAuthService twitchOAuthService, TwitchTokenSettingService twitchTokenSettingService, TwitchRefreshTokenSettingService twitchRefreshTokenSettingService, TwitchTokenExpiresAtSettingService twitchTokenExpiresAtSettingService)
        {
            _twitchOAuthService = twitchOAuthService;
            _twitchTokenSettingService = twitchTokenSettingService;
            _twitchRefreshTokenSettingService = twitchRefreshTokenSettingService;
            _twitchTokenExpiresAtSettingService = twitchTokenExpiresAtSettingService;
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

            var refreshed = await _twitchOAuthService.RefreshAccessTokenAsync(refreshToken);
            if (refreshed == null || string.IsNullOrEmpty(refreshed.AccessToken))
            {
                return;
            }

            // Persist the rotation details first, then publish the new token
            // through TwitchTokenService last, so watchers never see a new
            // token alongside a stale expiry.
            if (!string.IsNullOrEmpty(refreshed.RefreshToken))
            {
                _twitchRefreshTokenSettingService.SetRefreshToken(refreshed.RefreshToken);
            }
            _twitchTokenExpiresAtSettingService.SetTokenExpiresAt(DateTime.UtcNow.AddSeconds(refreshed.ExpiresIn).ToString("o"));
            _twitchTokenSettingService.SetToken(refreshed.AccessToken);
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