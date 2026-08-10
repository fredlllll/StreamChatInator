using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Hubs;

namespace StreamChatInator.Services
{
    public class ChatReaderService : BackgroundService
    {
        private readonly ILogger<ChatReaderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TwitchTokenService _tokenService;

        public ChatReaderService(ILogger<ChatReaderService> logger, IServiceScopeFactory scopeFactory, TwitchTokenService tokenService)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _tokenService = tokenService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Twitch Bot Service started. Waiting for authentication...");
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var hub = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

                try
                {
                    var channelName = db.GetSettingsValueOrNull(SettingValue.SettingUserName);
                    var oauthToken = await _tokenService.GetAccessTokenAsync(db);
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
    }
}