using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Hubs;
using System.Threading.Channels;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StreamChatInator.Services
{
    public class ChatReaderService : BackgroundService
    {
        private readonly ILogger<ChatReaderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatReaderService(ILogger<ChatReaderService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            this._scopeFactory = scopeFactory;
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
                    var channelName = db.GetSettingsValue(SettingValue.SettingUserName);
                    var oauthToken = db.GetSettingsValue(SettingValue.SettingOAuthToken);
                    db.Dispose();
                    db = null;

                    var reader = new ChatReader(channelName,oauthToken, _scopeFactory);
                    await reader.ConnectAsync();
                    await hub.Clients.All.SendAsync("Connection");
                    await reader.Run(stoppingToken);
                    await hub.Clients.All.SendAsync("NoConnection");
                }
                catch(Exception ex)
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
