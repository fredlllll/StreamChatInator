using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Services.Twitch;

namespace StreamChatInator.Services
{
    public class ChatReaderService : BackgroundService
    {
        private readonly ILogger<ChatReaderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TwitchTokenService _tokenService;
        private readonly ConfigService _config;

        public ChatReaderService(ILogger<ChatReaderService> logger, IServiceScopeFactory scopeFactory, TwitchTokenService tokenService, ConfigService config)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _tokenService = tokenService;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Twitch Bot Service started. Waiting for authentication...");
            ConsoleUi.SetStatus("Waiting for Twitch login…");
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var hubData = scope.ServiceProvider.GetRequiredService<ChatHubData>();
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

                    var reader = new ChatReader(_scopeFactory, channelName, oauthToken, _config.JoinChannel);
                    try
                    {
                        await reader.ConnectAsync();
                        hubData.SetConnected(true);
                        _logger.LogInformation("chat reader connected as {User}", channelName);
                        if (!string.IsNullOrWhiteSpace(_config.JoinChannel))
                        {
                            _logger.LogInformation("joining overridden channel {Channel} instead of own channel", _config.JoinChannel);
                        }
                        ConsoleUi.SetStatus($"Connected as {channelName}");
                        // Run returns when the twitch client drops or the app shuts down.
                        await reader.Run(stoppingToken);
                        _logger.LogInformation("chat reader disconnected");
                        ConsoleUi.SetStatus("Disconnected — reconnecting…");
                        hubData.SetConnected(false);
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
                    ConsoleUi.SetStatus("Connection failed — retrying…");
                    hubData.SetConnected(false);
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
    }
}