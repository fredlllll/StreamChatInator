using Acornima;
using Open.Observable;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Services.Twitch
{

    public class TwitchTokenService : SettingsObserverService
    {
        public ObservableValue<string?> Token => Value;
        public TwitchTokenService(IServiceScopeFactory scopeFactory) : base(SettingValue.SettingOAuthToken, scopeFactory) { }


        public string? GetAccessToken()
        {
            return GetValue();
        }

        public void SetAccessToken(string token)
        {
            SetValue(token);
        }

        public void UnsetAccessToken()
        {
            UnsetValue();
        }
    }
}
