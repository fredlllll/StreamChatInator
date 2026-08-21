using Open.Observable;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Services.Twitch.Settings
{

    public class TwitchTokenSettingService : SettingsObserverService
    {
        public ObservableValue<string?> Token => Value;
        public TwitchTokenSettingService(IServiceScopeFactory scopeFactory) : base(SettingValue.SettingOAuthToken, scopeFactory) { }


        public string? GetToken()
        {
            return GetValue();
        }

        public void SetToken(string token)
        {
            SetValue(token);
        }

        public void UnsetToken()
        {
            UnsetValue();
        }
    }
}
