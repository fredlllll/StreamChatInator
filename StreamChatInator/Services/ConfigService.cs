using Microsoft.Extensions.Options;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Typed facade over the app configuration. Sections are bound once to
    /// <see cref="TwitchOptions"/>/<see cref="AuthOptions"/> at registration,
    /// so config keys live in exactly one place (the Options classes) instead
    /// of being sprinkled through the services that consume them.
    /// </summary>
    public class ConfigService
    {
        private readonly TwitchOptions _twitch;
        private readonly AuthOptions _auth;

        public ConfigService(IOptions<TwitchOptions> twitch, IOptions<AuthOptions> auth)
        {
            _twitch = twitch.Value;
            _auth = auth.Value;
        }

        public string TwitchClientId => _twitch.ClientId ?? Constants.TwitchAppClientId;

        public string? TwitchJoinChannel => _twitch.JoinChannel;

        public bool AuthEnabled => _auth.Enabled;
        public string? AuthConfiguredPin => _auth.Pin;
    }
}
