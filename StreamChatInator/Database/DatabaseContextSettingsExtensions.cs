using Microsoft.EntityFrameworkCore;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Database
{
    /// <summary>
    /// Reads and writes rows in the settings (key/value) table. These are
    /// convenience accessors for the app's persisted settings (e.g. the Twitch
    /// OAuth token) rather than entity-model operations, so they live apart from
    /// <see cref="DatabaseContext"/> itself.
    /// </summary>
    public static class DatabaseContextSettingsExtensions
    {
        /// <summary>
        /// doesnt do save changes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetSettingsValue(this DatabaseContext db, string name, string value)
        {
            var existing = db.SettingValues.Find(name);
            if (existing != null)
            {
                existing.Value = value;
                existing.Updated = DateTime.UtcNow;
            }
            else
            {
                var setting = new SettingValue() { Id = name, Value = value };
                db.SettingValues.Add(setting);
            }
        }

        public static void UnsetSettingsValue(this DatabaseContext db, string name)
        {
            db.SettingValues.Where(x => x.Id == name).Take(1).ExecuteDelete();
        }

        public static string GetSettingsValue(this DatabaseContext db, string name)
        {
            var entry = db.SettingValues.Find(name);
            if (entry == null)
            {
                throw new Exception("no settings value for " + name);
            }
            return entry.Value;
        }

        public static string? GetSettingsValueOrNull(this DatabaseContext db, string name)
        {
            var entry = db.SettingValues.Find(name);
            return entry?.Value;
        }
    }
}