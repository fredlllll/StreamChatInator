using Microsoft.EntityFrameworkCore;
using StreamChatInator.Database.Models;

namespace StreamChatInator.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<SettingValue> SettingValues { get; set; }
        public DbSet<ChatEvent> ChatEvents { get; set; }
        public DbSet<ChatUserNoticeBase> ChatUserNoticeBases { get; set; }
        public DbSet<ChatEventFilter> ChatEventFilters { get; set; }


        public DbSet<ChatEventAnnouncement> ChatEventAnnouncements { get; set; }
        public DbSet<ChatEventAnonGiftPaidUpgrade> ChatEventAnonGiftPaidUpgrades { get; set; }
        public DbSet<ChatEventBitsBadgeTier> ChatEventBitsBadgeTiers { get; set; }
        public DbSet<ChatEventChatMessage> ChatEventChatMessages { get; set; }
        public DbSet<ChatEventCommunityPayForward> ChatEventCommunityPayForwards { get; set; }
        public DbSet<ChatEventCommunitySubscription> ChatEventCommunitySubscriptions { get; set; }
        public DbSet<ChatEventContinuedGiftedSubscription> ChatEventContinuedGiftedSubscriptions { get; set; }
        public DbSet<ChatEventGiftedSubscription> ChatEventGiftedSubscriptions { get; set; }
        public DbSet<ChatEventMessageCleared> ChatEventMessageCleareds { get; set; }
        public DbSet<ChatEventNewSubscriber> ChatEventNewSubscribers { get; set; }
        public DbSet<ChatEventPrimePaidSubscriber> ChatEventPrimePaidSubscribers { get; set; }
        public DbSet<ChatEventReSubscriber> ChatEventReSubscribers { get; set; }
        public DbSet<ChatEventRitual > ChatEventRituals { get; set; }
        public DbSet<ChatEventStandardPayForward> ChatEventStandardPayForwards { get; set; }
        public DbSet<ChatEventUserBanned> ChatEventUserBanneds { get; set; }
        public DbSet<ChatEventUserJoined> ChatEventUserJoineds { get; set; }
        public DbSet<ChatEventUserLeft> ChatEventUserLefts { get; set; }
        public DbSet<ChatEventUserTimedout> ChatEventUserTimedouts { get; set; }


        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingValue>().ToTable(nameof(SettingValues));
            modelBuilder.Entity<ChatEvent>().ToTable(nameof(ChatEvents));
            modelBuilder.Entity<ChatEvent>().HasIndex(e => e.Created);
            modelBuilder.Entity<ChatUserNoticeBase>().ToTable(nameof(ChatUserNoticeBases));
            modelBuilder.Entity<ChatEventFilter>().ToTable(nameof(ChatEventFilters));

            modelBuilder.Entity<ChatEventAnnouncement>().ToTable(nameof(ChatEventAnnouncements));
            modelBuilder.Entity<ChatEventAnonGiftPaidUpgrade>().ToTable(nameof(ChatEventAnonGiftPaidUpgrades));
            modelBuilder.Entity<ChatEventBitsBadgeTier>().ToTable(nameof(ChatEventBitsBadgeTiers));
            modelBuilder.Entity<ChatEventChatMessage>().ToTable(nameof(ChatEventChatMessages));
            modelBuilder.Entity<ChatEventCommunityPayForward>().ToTable(nameof(ChatEventCommunityPayForwards));
            modelBuilder.Entity<ChatEventCommunitySubscription>().ToTable(nameof(ChatEventCommunitySubscriptions));
            modelBuilder.Entity<ChatEventContinuedGiftedSubscription>().ToTable(nameof(ChatEventContinuedGiftedSubscriptions));
            modelBuilder.Entity<ChatEventGiftedSubscription>().ToTable(nameof(ChatEventGiftedSubscriptions));
            modelBuilder.Entity<ChatEventMessageCleared>().ToTable(nameof(ChatEventMessageCleareds));
            modelBuilder.Entity<ChatEventNewSubscriber>().ToTable(nameof(ChatEventNewSubscribers));
            modelBuilder.Entity<ChatEventPrimePaidSubscriber>().ToTable(nameof(ChatEventPrimePaidSubscribers));
            modelBuilder.Entity<ChatEventReSubscriber>().ToTable(nameof(ChatEventReSubscribers));
            modelBuilder.Entity<ChatEventRitual>().ToTable(nameof(ChatEventRituals));
            modelBuilder.Entity<ChatEventStandardPayForward>().ToTable(nameof(ChatEventStandardPayForwards));
            modelBuilder.Entity<ChatEventUserBanned>().ToTable(nameof(ChatEventUserBanneds));
            modelBuilder.Entity<ChatEventUserJoined>().ToTable(nameof(ChatEventUserJoineds));
            modelBuilder.Entity<ChatEventUserLeft>().ToTable(nameof(ChatEventUserLefts));
            modelBuilder.Entity<ChatEventUserTimedout>().ToTable(nameof(ChatEventUserTimedouts));
        }
    }
}
