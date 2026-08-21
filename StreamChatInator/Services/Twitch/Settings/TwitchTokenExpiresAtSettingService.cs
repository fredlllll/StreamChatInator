using Open.Observable;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Services.Twitch.Settings
{

    public class TwitchTokenExpiresAtSettingService : SettingsObserverService
    {
        public ObservableValue<string?> Token => Value;
        public TwitchTokenExpiresAtSettingService(IServiceScopeFactory scopeFactory) : base(SettingValue.SettingOAuthTokenExpiresAt, scopeFactory) { }


        public string? GetTokenExpiresAt()
        {
            return GetValue();
        }

        public void SetTokenExpiresAt(string token)
        {
            SetValue(token);
        }

        public void UnsetTokenExpiresAt()
        {
            UnsetValue();
        }
    }
}
