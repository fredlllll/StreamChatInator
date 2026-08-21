namespace StreamChatInator
{
    /// <summary>Strongly typed binding of the "Twitch" configuration section.</summary>
    public class TwitchOptions
    {
        public const string SectionName = "Twitch";

        /// <summary>Override for the Twitch application client id; falls back to <see cref="Constants.TwitchAppClientId"/>.</summary>
        public string? ClientId { get; set; }

        /// <summary>Channel to join instead of the logged-in user's own channel (testing aid).</summary>
        public string? JoinChannel { get; set; }
    }
}
