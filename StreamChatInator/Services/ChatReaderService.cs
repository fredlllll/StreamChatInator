using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Apis;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Hubs;
using System.Globalization;

namespace StreamChatInator.Services
{
    public class ChatReaderService : BackgroundService
    {
        private readonly ILogger<ChatReaderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;

        public ChatReaderService(ILogger<ChatReaderService> logger, IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _config = config;
        }

        private string ClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Twitch Bot Service started. Waiting for authentication...");
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var hub = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                try
                {
                    var channelName = db.GetSettingsValueOrNull(SettingValue.SettingUserName);
                    var oauthToken = await EnsureAccessTokenAsync(db, httpClientFactory);
                    if (string.IsNullOrEmpty(channelName) || string.IsNullOrEmpty(oauthToken))
                    {
                        throw new InvalidOperationException("no credentials available yet, waiting for login");
                    }
                    db.Dispose();
                    db = null;

                    var reader = new ChatReader(channelName, oauthToken, _scopeFactory);
                    await reader.ConnectAsync();
                    await hub.Clients.All.SendAsync("Connection");
                    await reader.Run(stoppingToken);
                    await hub.Clients.All.SendAsync("NoConnection");
                }
                catch (Exception)
                {
                    _logger.LogWarning("could not create chat reader");
                    await hub.Clients.All.SendAsync("NoConnection");
                }
                finally
                {
                    db?.Dispose();
                    await Task.Delay(2000);
                }
            }
        }

        /// <summary>
        /// Returns a usable access token, refreshing it via the stored refresh
        /// token when it is expired or its expiry is unknown. Returns null when
        /// no credentials exist yet or the refresh fails (a re-login is needed).
        /// </summary>
        private async Task<string?> EnsureAccessTokenAsync(DatabaseContext db, IHttpClientFactory httpClientFactory)
        {
            var token = db.GetSettingsValueOrNull(SettingValue.SettingOAuthToken);
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var refreshToken = db.GetSettingsValueOrNull(SettingValue.SettingOAuthRefreshToken);

            var expiresAt = DateTime.MinValue;
            var expiresRaw = db.GetSettingsValueOrNull(SettingValue.SettingOAuthTokenExpiresAt);
            if (!string.IsNullOrEmpty(expiresRaw) &&
                DateTime.TryParse(expiresRaw, null, DateTimeStyles.RoundtripKind, out var parsed))
            {
                expiresAt = parsed;
            }

            if (expiresAt != DateTime.MinValue && expiresAt > DateTime.UtcNow.AddMinutes(5))
            {
                return token;
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                return null;
            }

            var httpClient = httpClientFactory.CreateClient("twitch");
            var refreshed = await Twitch.RefreshAccessTokenAsync(httpClient, ClientId, refreshToken);
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
    }
}