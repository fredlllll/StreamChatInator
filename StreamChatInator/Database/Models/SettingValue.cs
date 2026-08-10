namespace StreamChatInator.Database.Models
{
    public class SettingValue : Model
    {
        public const string SettingOAuthToken = "oauthtoken";
        public const string SettingOAuthRefreshToken = "oauthrefreshtoken";
        public const string SettingOAuthTokenExpiresAt = "oauthtokenexpiresat";
        public const string SettingUserName = "username";


        public required string Value { get; set; }
    }
}
