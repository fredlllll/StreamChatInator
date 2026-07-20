using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<SettingValue> SettingValues { get; set; }
        public DbSet<ChatEvent> ChatEvents { get; set; }
        public DbSet<ChatEventMessage> ChatEventsMessages { get; set; }
        public DbSet<ChatEventFilter> EventFilters { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingValue>().ToTable(nameof(SettingValues));
            modelBuilder.Entity<ChatEvent>().ToTable(nameof(ChatEvents));
            modelBuilder.Entity<ChatEventMessage>().ToTable(nameof(ChatEventsMessages));
            modelBuilder.Entity<ChatEventFilter>().ToTable(nameof(EventFilters));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source=db.sqlite");
        }

        /// <summary>
        /// doesnt do save changes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetSettingsValue(string name, string value)
        {
            var existing = SettingValues.Find(name);
            if (existing != null)
            {
                existing.Value = value;
                existing.Updated = DateTime.UtcNow;
            }
            else
            {
                var setting = new SettingValue() { Id = name, Value = value };
                SettingValues.Add(setting);
            }
        }

        public string GetSettingsValue(string name)
        {
            var entry = SettingValues.Find(name);
            if (entry == null)
            {
                throw new Exception("no settings value for " + name);
            }
            return entry.Value;
        }
    }
}
