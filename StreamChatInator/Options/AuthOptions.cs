namespace StreamChatInator
{
    /// <summary>Strongly typed binding of the "Auth" configuration section.</summary>
    public class AuthOptions
    {
        public const string SectionName = "Auth";

        /// <summary>Whether browsers must unlock the UI via PIN/session cookie. Opt-out via <c>Auth:Enabled=false</c>.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Fixed PIN to accept instead of the generated one shown on the console panel.</summary>
        public string? Pin { get; set; }
    }
}
