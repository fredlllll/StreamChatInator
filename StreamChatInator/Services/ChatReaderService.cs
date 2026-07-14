using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Threading.Channels;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Interfaces;

namespace StreamChatInator.Services
{
    public class ChatReaderService : BackgroundService
    {
        private readonly ILogger<ChatReaderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public ChatReaderService(ILogger<ChatReaderService> logger, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            this._scopeFactory = scopeFactory;
            this._loggerFactory = loggerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Twitch Bot Service started. Waiting for authentication...");
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                try
                {
                    var channelName = db.GetSettingsValue(SettingValue.SettingUserName);
                    var oauthToken = db.GetSettingsValue(SettingValue.SettingOAuthToken);
                    db.Dispose();
                    db = null;

                    var reader = new ChatReader(channelName,oauthToken, _scopeFactory, _loggerFactory);
                    await reader.Run(stoppingToken);
                }
                catch
                {
                    //no value found
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
