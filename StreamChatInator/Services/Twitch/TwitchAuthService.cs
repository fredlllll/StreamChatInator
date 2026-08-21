using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Globalization;

namespace StreamChatInator.Services.Twitch
{
    /// <summary>
    /// Owns the Twitch credential lifecycle: login signaling, persisting fresh
    /// credentials, and keeping the stored token refreshed (via
    /// <see cref="TwitchTokenRefreshService"/>). Consumers read the current
    /// token from <see cref="TwitchTokenService"/> - they never need this class.
    /// </summary>
    public class TwitchAuthService
    {
        // Async latch: once a device login has persisted credentials it stays
        // signaled (so a steady-state reconnect doesn't wait). ResetCredentialWait
        // swaps in a fresh, unsignaled latch when stored credentials become
        // unusable, blocking until the next login.
        private TaskCompletionSource _loginReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TwitchOAuthClient _twitchOAuthClient;
        private readonly TwitchTokenService _twitchTokenService;

        public TwitchAuthService(TwitchOAuthClient twitchOAuthClient, IServiceScopeFactory scopeFactory, TwitchTokenService twitchTokenService)
        {
            _twitchOAuthClient = twitchOAuthClient;
            _scopeFactory = scopeFactory;
            _twitchTokenService = twitchTokenService;
        }

        /// <summary>Called after credentials are persisted (device login completed).</summary>
        public void SignalLogin()
        {
            var current = Volatile.Read(ref _loginReady);
            if (!current.Task.IsCompleted)
            {
                current.TrySetResult();
            }
        }

        /// <summary>Makes credential waits block again, e.g. after a token can't be refreshed.</summary>
        public void ResetCredentialWait()
        {
            Interlocked.Exchange(ref _loginReady, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
        }

        /// <summary>Returns when credentials exist; completes immediately once a login has happened.</summary>
        public Task WaitForCredentialsAsync(CancellationToken cancellationToken)
        {
            var current = Volatile.Read(ref _loginReady);
            if (current.Task.IsCompleted) return Task.CompletedTask;
            return WaitOnAsync(current, cancellationToken);
        }

        private static async Task WaitOnAsync(TaskCompletionSource ready, CancellationToken cancellationToken)
        {
            var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = cancellationToken.Register(() => cancelled.TrySetResult());
            await Task.WhenAny(ready.Task, cancelled.Task);
            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Refreshes the stored token when it is inside the expiry window.
        /// Called periodically by <see cref="TwitchTokenRefreshService"/> so
        /// consumers reading the token from <see cref="TwitchTokenService"/>
        /// always get a usable one without refresh logic of their own. No-op
        /// when no credentials exist or the token is still fresh.
        /// </summary>
        public async Task EnsureFreshTokenAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var token = _twitchTokenService.GetAccessToken();
            if (string.IsNullOrEmpty(token) || !NeedsRefresh(db))
            {
                return;
            }

            await RefreshAsync(db);
        }

        private async Task RefreshAsync(DatabaseContext db)
        {
            var refreshToken = db.GetSettingsValueOrNull(SettingValue.SettingOAuthRefreshToken);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return;
            }

            var refreshed = await _twitchOAuthClient.RefreshAccessTokenAsync(refreshToken);
            if (refreshed == null || string.IsNullOrEmpty(refreshed.AccessToken))
            {
                return;
            }

            // Persist the rotation details first, then publish the new token
            // through TwitchTokenService last, so watchers never see a new
            // token alongside a stale expiry.
            if (!string.IsNullOrEmpty(refreshed.RefreshToken))
            {
                db.SetSettingsValue(SettingValue.SettingOAuthRefreshToken, refreshed.RefreshToken);
            }
            db.SetSettingsValue(SettingValue.SettingOAuthTokenExpiresAt, DateTime.UtcNow.AddSeconds(refreshed.ExpiresIn).ToString("o"));
            db.SaveChanges();

            _twitchTokenService.SetAccessToken(refreshed.AccessToken);
        }

        private static bool NeedsRefresh(DatabaseContext db)
        {
            var expiresRaw = db.GetSettingsValueOrNull(SettingValue.SettingOAuthTokenExpiresAt);
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