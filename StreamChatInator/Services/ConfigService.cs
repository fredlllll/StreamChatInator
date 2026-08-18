namespace StreamChatInator.Services
{
    public class ConfigService
    {
        private IConfiguration _config;

        public string TwitchClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

        public string? TwitchJoinChannel => _config["Twitch:JoinChannel"];

        public bool AuthEnabled => _config.GetValue("Auth:Enabled", true);
        public string? AuthConfiguredPin => _config["Auth:Pin"];

        public ConfigService(IConfiguration config)
        {
            _config = config;
        }
    }
}
