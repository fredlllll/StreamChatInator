using StreamChatInator.Services.Twitch;
using StreamChatInator.Services.Twitch.Settings;

namespace StreamChatInator.Services
{
    public class ChatReaderService : BackgroundService
    {
        private readonly ILogger<ChatReaderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TwitchTokenSettingService _tokenService;
        private readonly TwitchUsernameService _usernameService;
        private readonly ConfigService _config;

        public ChatReaderService(ILogger<ChatReaderService> logger, IServiceScopeFactory scopeFactory, TwitchTokenSettingService tokenService, TwitchUsernameService usernameService, ConfigService config)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _tokenService = tokenService;
            _usernameService = usernameService;
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
                    var channelName = _usernameService.GetUsername();
                    var oauthToken = _tokenService.GetToken();

                    if (string.IsNullOrEmpty(channelName))
                    {
                        await _usernameService.WaitOnValueAsync(stoppingToken);
                        channelName = _usernameService.GetUsername()!;
                    }
                    if (string.IsNullOrEmpty(oauthToken))
                    {
                        await _tokenService.WaitOnValueAsync(stoppingToken);
                        oauthToken = _tokenService.GetToken()!;
                    }

                    string? joinChannel = _config.TwitchJoinChannel;
                    if (!string.IsNullOrWhiteSpace(joinChannel))
                    {
                        _logger.LogInformation("joining overridden channel {Channel} instead of own channel", joinChannel);
                    }

                    var reader = new ChatReader(_scopeFactory, channelName, oauthToken, joinChannel);
                    try
                    {
                        await reader.ConnectAsync();
                        hubData.SetConnected(true);
                        _logger.LogInformation("chat reader connected as {User}", channelName);
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