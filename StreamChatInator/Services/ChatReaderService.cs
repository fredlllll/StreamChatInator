using StreamChatInator.Services.Twitch;
using StreamChatInator.Services.Twitch.Settings;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Owns the Twitch chat connection lifecycle: waits for credentials,
    /// connects, runs until the client drops or the app shuts down, and decides
    /// how to reconnect. A failed login (dead token) makes it wait for a fresh
    /// login instead of hammering Twitch with doomed reconnect attempts; any
    /// other drop retries after a short delay.
    /// </summary>
    public class ChatReaderService : BackgroundService
    {
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(2);

        private readonly ILogger<ChatReaderService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ChatHubData _hubData;
        private readonly TwitchClientEventAdapter _eventAdapter;
        private readonly TwitchTokenSettingService _tokenService;
        private readonly TwitchUsernameService _usernameService;
        private readonly TwitchOAuthService _oauthService;
        private readonly ConfigService _config;

        public ChatReaderService(
            ILogger<ChatReaderService> logger,
            ILoggerFactory loggerFactory,
            ChatHubData hubData,
            TwitchClientEventAdapter eventAdapter,
            TwitchTokenSettingService tokenService,
            TwitchUsernameService usernameService,
            TwitchOAuthService oauthService,
            ConfigService config)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _hubData = hubData;
            _eventAdapter = eventAdapter;
            _tokenService = tokenService;
            _usernameService = usernameService;
            _oauthService = oauthService;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Twitch Bot Service started. Waiting for authentication...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (username, token) = await WaitForCredentialsAsync(stoppingToken);

                    try
                    {
                        await RunConnectionAsync(username, token, stoppingToken);
                        _logger.LogInformation("chat reader disconnected");
                        ConsoleUi.SetStatus("Disconnected — reconnecting…");
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "chat reader failed");
                        ConsoleUi.SetStatus("Connection failed — checking login…");
                    }

                    // The run ended. If the stored token no longer validates, the
                    // user must log in again - wait for new credentials instead of
                    // retrying with a known-dead token every two seconds.
                    if (!await IsTokenUsableAsync(token, stoppingToken))
                    {
                        _logger.LogWarning("twitch token is no longer valid; waiting for re-login");
                        ConsoleUi.SetStatus("Login expired — waiting for new Twitch login…");
                        await _tokenService.WaitOnChangeFromAsync(token, stoppingToken);
                        continue;
                    }

                    // Transient failure (or clean drop): pause briefly, but skip the
                    // delay when credentials changed in the meantime.
                    if (!CredentialsChanged(username, token))
                    {
                        await Task.Delay(ReconnectDelay, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<(string Username, string Token)> WaitForCredentialsAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                var username = _usernameService.GetUsername();
                if (string.IsNullOrEmpty(username))
                {
                    ConsoleUi.SetStatus("Waiting for Twitch login…");
                    await _usernameService.WaitOnValueAsync(stoppingToken);
                    continue;
                }

                var token = _tokenService.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    ConsoleUi.SetStatus("Waiting for Twitch login…");
                    await _tokenService.WaitOnValueAsync(stoppingToken);
                    continue;
                }

                return (username, token);
            }
        }

        private async Task<bool> IsTokenUsableAsync(string token, CancellationToken stoppingToken)
        {
            try
            {
                return await _oauthService.ValidateTokenAsync(token) != null;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Validation itself failing (network hiccup etc.) says nothing
                // about the token; treat it as usable and let the retry loop run.
                _logger.LogWarning(ex, "could not validate twitch token");
                return true;
            }
        }

        private bool CredentialsChanged(string username, string token)
        {
            return !string.Equals(_usernameService.GetUsername(), username, StringComparison.Ordinal)
                   || !string.Equals(_tokenService.GetToken(), token, StringComparison.Ordinal);
        }

        private async Task RunConnectionAsync(string username, string token, CancellationToken stoppingToken)
        {
            // Lets testing point the bot at a channel other than the logged-in
            // one; empty falls back to the logged-in user's own channel.
            string? overrideChannel = _config.TwitchJoinChannel;
            if (!string.IsNullOrWhiteSpace(overrideChannel))
            {
                _logger.LogInformation("joining overridden channel {Channel} instead of own channel", overrideChannel);
            }
            var joinChannel = string.IsNullOrWhiteSpace(overrideChannel) ? username : overrideChannel;

            var disconnected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var client = new TwitchClient(loggerFactory: _loggerFactory);
            client.Initialize(new ConnectionCredentials(username, token));
            _eventAdapter.Attach(client);

            client.OnConnected += async (_, _) =>
            {
                _logger.LogInformation("twitch client connected as " + username);
                await client.JoinChannelAsync(joinChannel);
            };
            client.OnJoinedChannel += (_, e) =>
            {
                _logger.LogInformation("twitch client joined channel " + e.Channel + " as " + e.BotUsername);
                return Task.CompletedTask;
            };
            client.OnDisconnected += (_, _) =>
            {
                _logger.LogInformation("twitch client disconnected");
                disconnected.TrySetResult();
                return Task.CompletedTask;
            };
            client.OnChatCommandReceived += HandleChatCommand;

            await client.ConnectAsync();
            _hubData.SetConnected(true);
            _logger.LogInformation("chat reader connected as {User}", username);
            ConsoleUi.SetStatus($"Connected as {username}");

            try
            {
                // Return when the twitch client drops so we can reconnect,
                // or when the app is shutting down.
                using var registration = stoppingToken.Register(() => disconnected.TrySetResult());
                await disconnected.Task;
            }
            finally
            {
                _hubData.SetConnected(false);
                try
                {
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "error disconnecting twitch client");
                }
            }
        }

        private Task HandleChatCommand(object? sender, OnChatCommandReceivedArgs e)
        {
            var command = e.Command;

            // Only the channel owner (the Twitch account that authorized the app)
            // may pause/resume tracking; otherwise anyone in chat could stop
            // recording. The bot only joins its own channel, so IsBroadcaster is
            // only ever true for the logged-in account.
            if (!e.ChatMessage.IsBroadcaster) return Task.CompletedTask;

            switch (command.Name.ToLower())
            {
                case "stoptracking":
                    SetTracking(false);
                    _logger.LogInformation("tracking stopped by {User}", e.ChatMessage.Username);
                    break;
                case "starttracking":
                    SetTracking(true);
                    _logger.LogInformation("tracking started by {User}", e.ChatMessage.Username);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets whether chat events are recorded and broadcast, notifying any
        /// connected frontends so their pause/play button stays in sync.
        /// </summary>
        private void SetTracking(bool enabled) => _hubData.Tracking.Post(enabled);
    }
}
