using System.Text.Json;

namespace StreamChatInator
{
    /// <summary>
    /// Serializes a TwitchLib badge list (set/version pairs) into a JSON array
    /// of {"set": "...", "version": "..."} objects for the client, or null
    /// if there are no badges. Badges with no version default to version "0".
    /// </summary>
    public static class BadgeSerializer
    {
        public static string? SerializeBadges(List<KeyValuePair<string, string>>? badges)
        {
            if (badges is null || badges.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(
                badges
                    .Where(b => !string.IsNullOrWhiteSpace(b.Key))
                    .Select(b => new { set = b.Key, version = string.IsNullOrEmpty(b.Value) ? "0" : b.Value }));
        }
    }
}