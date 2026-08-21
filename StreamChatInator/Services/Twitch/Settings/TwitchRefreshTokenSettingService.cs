using Open.Observable;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Services.Twitch.Settings
{

    public class TwitchRefreshTokenSettingService : SettingsObserverService
    {
        public ObservableValue<string?> Token => Value;
        public TwitchRefreshTokenSettingService(IServiceScopeFactory scopeFactory) : base(SettingValue.SettingOAuthRefreshToken, scopeFactory) { }


        public string? GetRefreshToken()
        {
            return GetValue();
        }

        public void SetRefreshToken(string token)
        {
            SetValue(token);
        }

        public void UnsetRefreshToken()
        {
            UnsetValue();
        }
    }
}
