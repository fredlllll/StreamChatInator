using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Globalization;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Owns reading, persisting and refreshing the Twitch OAuth access token
    /// stored in the settings table. Both the chat reader and other services
    /// that call the Twitch API (e.g. badge/emote fetchers) depend on this
    /// instead of duplicating the refresh logic.
    /// </summary>
    public class TwitchTokenService
    {
        // Async latch: once a device login has persisted credentials it stays
        // signaled (so a steady-state reconnect doesn't wait). ResetCredentialWait
        // swaps in a fresh, unsignaled latch when stored credentials become
        // unusable, blocking until the next login.
        private TaskCompletionSource _loginReady = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TwitchApiService _twitchApiService;

        public TwitchTokenService(IHttpClientFactory httpClientFactory, TwitchApiService twitchApiService, IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _httpClientFactory = httpClientFactory;
            _twitchApiService = twitchApiService;
            _config = config;
            _scopeFactory = scopeFactory;
        }

        private string ClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

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
        /// Returns a usable access token, creating its own scope. Null when no
        /// credentials exist or the refresh fails (a re-login is needed).
        /// </summary>
        public async Task<string?> GetAccessTokenAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            return await GetAccessTokenAsync(db);
        }

        /// <summary>
        /// Returns a usable access token using a caller-owned context (e.g. the
        /// scoped context a background service already has open).
        /// </summary>
        public async Task<string?> GetAccessTokenAsync(DatabaseContext db)
        {
            var token = db.GetSettingsValueOrNull(SettingValue.SettingOAuthToken);
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            if (!NeedsRefresh(db))
            {
                return token;
            }

            var refreshed = await RefreshAsync(db);
            if (!string.IsNullOrEmpty(refreshed))
            {
                return refreshed;
            }

            // Refresh failed (e.g. Twitch's API is briefly unreachable or the
            // refresh token was revoked, but the access token still works). Keep
            // using the stored token while it's still within its expiry window
            // instead of forcing a full re-login; only give up once it's actually
            // expired and would 401 anyway.
            return TokenStillValid(db) ? token : null;
        }

        private static bool TokenStillValid(DatabaseContext db)
        {
            var expiresRaw = db.GetSettingsValueOrNull(SettingValue.SettingOAuthTokenExpiresAt);
            if (string.IsNullOrEmpty(expiresRaw)) return false;
            if (!DateTime.TryParse(expiresRaw, null, DateTimeStyles.RoundtripKind, out var expires)) return false;
            return expires > DateTime.UtcNow;
        }

        /// <summary>
        /// Forces a token refresh in a fresh scope, even when the stored expiry
        /// still looks valid. Useful to recover from a 401 where the token was
        /// revoked outside its expiry window.
        /// </summary>
        public async Task<string?> RefreshAccessTokenAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            return await RefreshAsync(db);
        }

        private async Task<string?> RefreshAsync(DatabaseContext db)
        {
            var refreshToken = db.GetSettingsValueOrNull(SettingValue.SettingOAuthRefreshToken);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return null;
            }

            var refreshed = await _twitchApiService.RefreshAccessTokenAsync(ClientId, refreshToken);
            if (refreshed == null || string.IsNullOrEmpty(refreshed.AccessToken))
            {
                return null;
            }

            db.SetSettingsValue(SettingValue.SettingOAuthToken, refreshed.AccessToken);
            if (!string.IsNullOrEmpty(refreshed.RefreshToken))
            {
                db.SetSettingsValue(SettingValue.SettingOAuthRefreshToken, refreshed.RefreshToken);
            }
            db.SetSettingsValue(SettingValue.SettingOAuthTokenExpiresAt, DateTime.UtcNow.AddSeconds(refreshed.ExpiresIn).ToString("o"));
            db.SaveChanges();

            return refreshed.AccessToken;
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