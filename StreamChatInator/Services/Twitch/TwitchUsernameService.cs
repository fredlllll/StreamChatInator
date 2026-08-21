using Open.Observable;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Services.Twitch
{
    public class TwitchUsernameService : SettingsObserverService
    {
        public ObservableValue<string?> Username => Value;
        public TwitchUsernameService(IServiceScopeFactory scopeFactory) : base(SettingValue.SettingUserName, scopeFactory) { }


        public string? GetUsername()
        {
            return GetValue();
        }

        public void SetUsername(string token)
        {
            SetValue(token);
        }

        public void UnsetUsername()
        {
            UnsetValue();
        }
    }
}
