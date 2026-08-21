namespace StreamChatInator
{
    public class Constants
    {
        public const string TwitchAppClientId = "a7e4krn5zk9e6b9wvlh2i3by47gmjp";

        /// <summary>
        /// Name of TwitchLib's private badge/flags field on UserDetails, read
        /// via reflection when persisting chat events. Breaks silently if
        /// TwitchLib renames it - hence one named constant instead of literals.
        /// </summary>
        public const string TwitchLibUserFlagsFieldName = "_flags";
    }
}
