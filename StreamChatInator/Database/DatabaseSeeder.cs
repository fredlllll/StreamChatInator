using StreamChatInator.Database.Models;

namespace StreamChatInator.Database
{
    /// <summary>Inserts a set of sensible default filters the first time the app runs.</summary>
    public static class DatabaseSeeder
    {
        public static void Seed(DatabaseContext db)
        {
            if (db.EventFilters.Any()) return;

            foreach (var f in Defaults)
            {
                db.EventFilters.Add(new ChatEventFilter
                {
                    Id = Model.GetNewId<ChatEventFilter>(),
                    Name = f.Name,
                    Code = f.Code,
                    CodeJs = f.CodeJs,
                });
            }
            db.SaveChanges();
        }

        private sealed record FilterSpec(string Name, string Code, string CodeJs);

        private static readonly FilterSpec[] Defaults =
        {
            new(
                "Everything",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    return true;
                }
                """,
                """
                function __matches(eventData) {
                    return true;
                }
                """),

            new(
                "Unseen only",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    return !eventData.seen;
                }
                """,
                """
                function __matches(eventData) {
                    return !eventData.seen;
                }
                """),

            new(
                "Just messages",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    return eventData.chatEventType === "ChatMessage";
                }
                """,
                """
                function __matches(eventData) {
                    return eventData.chatEventType === "ChatMessage";
                }
                """),

            new(
                "Mods & staff",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    var d = eventData.chatEventData;
                    return d != null && d.userTypeName != null && ["Moderator", "GlobalModerator", "Broadcaster", "Admin", "Staff"].includes(d.userTypeName);
                }
                """,
                """
                function __matches(eventData) {
                    var d = eventData.chatEventData;
                    return d != null && d.userTypeName != null && ["Moderator", "GlobalModerator", "Broadcaster", "Admin", "Staff"].includes(d.userTypeName);
                }
                """),

            new(
                "Subs & gifts",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    var t = eventData.chatEventType;
                    return t === "NewSubscriber" || t === "ReSubscriber" || t === "PrimePaidSubscriber"
                        || t === "CommunitySubscription" || t === "GiftedSubscription" || t === "AnonGiftPaidUpgrade"
                        || t === "ContinuedGiftedSubscription" || t === "StandardPayForward" || t === "CommunityPayForward"
                        || t === "BitsBadgeTier";
                }
                """,
                """
                function __matches(eventData) {
                    var t = eventData.chatEventType;
                    return t === "NewSubscriber" || t === "ReSubscriber" || t === "PrimePaidSubscriber"
                        || t === "CommunitySubscription" || t === "GiftedSubscription" || t === "AnonGiftPaidUpgrade"
                        || t === "ContinuedGiftedSubscription" || t === "StandardPayForward" || t === "CommunityPayForward"
                        || t === "BitsBadgeTier";
                }
                """),

            new(
                "Cheers & bits",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    if (eventData.chatEventType === "BitsBadgeTier") return true;
                    return eventData.chatEventType === "ChatMessage" && eventData.chatEventData.bits > 0;
                }
                """,
                """
                function __matches(eventData) {
                    if (eventData.chatEventType === "BitsBadgeTier") return true;
                    return eventData.chatEventType === "ChatMessage" && eventData.chatEventData.bits > 0;
                }
                """),

            new(
                "First-time chatters",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    return eventData.chatEventType === "ChatMessage" && eventData.chatEventData.isFirstMessage === true;
                }
                """,
                """
                function __matches(eventData) {
                    return eventData.chatEventType === "ChatMessage" && eventData.chatEventData.isFirstMessage === true;
                }
                """),

            new(
                "No join/leave noise",
                """
                function __matches(eventData: ChatEventEnvelope): boolean {
                    return eventData.chatEventType !== "UserJoined" && eventData.chatEventType !== "UserLeft";
                }
                """,
                """
                function __matches(eventData) {
                    return eventData.chatEventType !== "UserJoined" && eventData.chatEventType !== "UserLeft";
                }
                """),
        };
    }
}
