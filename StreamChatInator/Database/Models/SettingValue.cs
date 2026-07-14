namespace StreamChatInator.Database.Models
{
    public class SettingValue : Model
    {
        public const string SettingOAuthToken = "oauthtoken";
        public const string SettingUserName = "username";


        public required string Value { get; set; }
    }
}
