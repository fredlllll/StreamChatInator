namespace StreamChatInator.Services
{
    public class ConfigService
    {
        private IConfiguration _config;

        public string ClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

        public string? JoinChannel => _config["Twitch:JoinChannel"];

        public ConfigService(IConfiguration config)
        {
            _config = config;
        }
    }
}
