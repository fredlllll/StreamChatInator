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
                var hub = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var channelName = db.GetSettingsValueOrNull(SettingValue.SettingUserName);
                    var oauthToken = await _tokenService.GetAccessTokenAsync(db);

                    if (string.IsNullOrEmpty(channelName) || string.IsNullOrEmpty(oauthToken))
                    {
                        // No usable credentials: block until a login completes
                        // instead of polling the settings table. The cancel token
                        // wakes us on shutdown.
                        _tokenService.ResetCredentialWait();
                        try
                        {
                            await _tokenService.WaitForCredentialsAsync(stoppingToken);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        continue;
                    }

                    db.Dispose();

                    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                    var hubData = scope.ServiceProvider.GetRequiredService<ChatHubData>();
                    var reader = new ChatReader(channelName, oauthToken, loggerFactory, hub, hubData, _scopeFactory);
                    try
                    {
                        await reader.ConnectAsync();
                        await hub.Clients.All.SendAsync("Connection", stoppingToken);
                        _logger.LogInformation("chat reader connected as {User}", channelName);
                        // Run returns when the twitch client drops or the app shuts down.
                        await reader.Run(stoppingToken);
                        _logger.LogInformation("chat reader disconnected");
                        await hub.Clients.All.SendAsync("NoConnection", stoppingToken);
                    }
                    finally
                    {
                        await reader.DisposeAsync();
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "chat reader failed, will retry");
                    await TrySendNoConnectionAsync(hub, stoppingToken);
                }
                finally
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }

        private static async Task TrySendNoConnectionAsync(IHubContext<ChatHub> hub, CancellationToken stoppingToken)
        {
            try
            {
                await hub.Clients.All.SendAsync("NoConnection", stoppingToken);
            }
            catch (Exception)
            {
                // broadcasting during shutdown is non-fatal
            }
        }
    }
}