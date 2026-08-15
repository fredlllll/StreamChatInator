using StreamChatInator.Database.Models;
using TwitchLib.Client.Enums;

namespace StreamChatInator.Services
{
    /// <summary>
    /// Builds one synthetic event of every chat event type so the frontend's
    /// rendering of each can be inspected without waiting for real Twitch
    /// traffic. Test data is wired through the same save + broadcast path as
    /// real events, so visuals (and any filters) behave exactly as they would
    /// with live data.
    /// </summary>
    public static class TestEventFactory
    {
        private static readonly DateTime Now = DateTime.UtcNow;

        /// <summary>One synthetic event: the type plus its per-type detail row and optional shared user-notice base row.</summary>
        public record TestEvent(ChatEventType Type, Model Data, Model? SubData);

        public static IEnumerable<TestEvent> CreateAll()
        {
            yield return CreateChatMessage();
            yield return CreateAnnouncement();
            yield return CreateAnonGiftPaidUpgrade();
            yield return CreateBitsBadgeTier();
            yield return CreateCommunityPayForward();
            yield return CreateCommunitySubscription();
            yield return CreateContinuedGiftedSubscription();
            yield return CreateGiftedSubscription();
            yield return CreateMessageCleared();
            yield return CreateNewSubscriber();
            yield return CreatePrimePaidSubscriber();
            yield return CreateReSubscriber();
            yield return CreateRitual();
            yield return CreateStandardPayForward();
            yield return CreateUserBanned();
            yield return CreateUserJoined();
            yield return CreateUserLeft();
            yield return CreateUserTimedout();
        }

        static TestEvent CreateChatMessage()
        {
            return new TestEvent(ChatEventType.ChatMessage, new ChatEventChatMessage()
            {
                Id = Model.GetNewId<ChatEventChatMessage>(),
                Bits = 100,
                BitsInDollars = 1.40,
                Emotes = null,
                CustomRewardId = null,
                TwitchMessageId = "test-msg-1",
                IsBroadcaster = false,
                IsFirstMessage = true,
                IsHighlighted = false,
                IsMe = false,
                IsSkippingSubMode = false,
                Message = "Hey everyone! First time in this channel, love the vibe!",
                Noisy = Noisy.NotSet,
                SubscribedMonthCount = 6,
                TmiSent = Now,
                ReplyParentMessageTwitchMessageId = null,
                DisplayName = "AliceTheMod",
                UserId = "1001",
                Username = "alicethemod",
                HexColor = "#00FF7F",
                Badges = BadgeSerializer.SerializeBadges(new List<KeyValuePair<string, string>> { new("moderator", "1"), new("subscriber", "6") }),
                UserFlags = UserDetails.Moderator | UserDetails.Subscriber | UserDetails.Vip,
                UserType = UserType.Moderator,
            }, null);
        }

        static TestEvent CreateAnnouncement()
        {
            var baseRow = MakeNoticeBase("announcer", "Announcer", "announcement", "announcer made an announcement");
            return new TestEvent(ChatEventType.Announcement, new ChatEventAnnouncement()
            {
                Id = Model.GetNewId<ChatEventAnnouncement>(),
                MsgParamColor = "PRIMARY",
                Message = "Check out the new schedule - streams every Tuesday and Friday!",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateAnonGiftPaidUpgrade()
        {
            var baseRow = MakeNoticeBase("anonupgrader", "Anonymous", "anongiftpaidupgrade", "an anonymous user upgraded their gifted sub");
            return new TestEvent(ChatEventType.AnonGiftPaidUpgrade, new ChatEventAnonGiftPaidUpgrade()
            {
                Id = Model.GetNewId<ChatEventAnonGiftPaidUpgrade>(),
                MsgParamPromoGiftTotal = 5,
                MsgParamPromoName = "Subtember 2026",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateBitsBadgeTier()
        {
            var baseRow = MakeNoticeBase("bitbadder", "BitBadder", "bitsbadgetier", "BitBadder earned the 1000-bit badge");
            return new TestEvent(ChatEventType.BitsBadgeTier, new ChatEventBitsBadgeTier()
            {
                Id = Model.GetNewId<ChatEventBitsBadgeTier>(),
                MsgParamThreshold = 1000,
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateCommunityPayForward()
        {
            var baseRow = MakeNoticeBase("payforwarder", "PayForwarder", "communitypayforward", "PayForwarder is paying forward a gift from GifterOne to the community");
            return new TestEvent(ChatEventType.CommunityPayForward, new ChatEventCommunityPayForward()
            {
                Id = Model.GetNewId<ChatEventCommunityPayForward>(),
                MsgParamPriorGifterAnonymous = false,
                MsgParamPriorGifterDisplayName = "GifterOne",
                MsgParamPriorGifterId = "2001",
                MsgParamPriorGifterUserName = "gifterone",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateCommunitySubscription()
        {
            var baseRow = MakeNoticeBase("massgifter", "MassGifter", "subgift", "MassGifter gifted 10 subs to the community");
            return new TestEvent(ChatEventType.CommunitySubscription, new ChatEventCommunitySubscription()
            {
                Id = Model.GetNewId<ChatEventCommunitySubscription>(),
                IsAnonymous = false,
                MsgParamGiftTheme = "TwitchCon",
                MsgParamMassGiftCount = 10,
                MsgParamOriginId = "test-origin-1",
                MsgParamSenderCount = 25,
                MsgParamSubPlan = SubscriptionPlan.Tier1,
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateContinuedGiftedSubscription()
        {
            var baseRow = MakeNoticeBase("continued", "ContinuedGifter", "subgift", "ContinuedGifter's gifted sub to RecipientOne was renewed");
            return new TestEvent(ChatEventType.ContinuedGiftedSubscription, new ChatEventContinuedGiftedSubscription()
            {
                Id = Model.GetNewId<ChatEventContinuedGiftedSubscription>(),
                MsgParamPromoGiftTotal = 3,
                MsgParamPromoName = "Subtember 2026",
                MsgParamSenderUsername = "gifterone",
                MsgParamSenderName = "GifterOne",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateGiftedSubscription()
        {
            var baseRow = MakeNoticeBase("recipientone", "RecipientOne", "subgift", "GifterOne gifted a Tier 1 sub to RecipientOne!");
            return new TestEvent(ChatEventType.GiftedSubscription, new ChatEventGiftedSubscription()
            {
                Id = Model.GetNewId<ChatEventGiftedSubscription>(),
                IsAnonymous = false,
                MsgParamMonths = 1,
                MsgParamOriginId = "test-origin-2",
                MsgParamRecipientDisplayName = "RecipientOne",
                MsgParamRecipientId = "3001",
                MsgParamRecipientUserName = "recipientone",
                MsgParamSenderCount = 2,
                MsgParamSubPlan = SubscriptionPlan.Tier1,
                MsgParamSubPlanName = "Tier 1",
                MsgParamMultiMonthGiftDuration = 3,
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateMessageCleared()
        {
            return new TestEvent(ChatEventType.MessageCleared, new ChatEventMessageCleared()
            {
                Id = Model.GetNewId<ChatEventMessageCleared>(),
                Channel = "testchannel",
                Message = "This message was deleted by a moderator",
                TargetTwitchMessageId = "test-msg-2",
                TmiSent = Now,
            }, null);
        }

        static TestEvent CreateNewSubscriber()
        {
            var baseRow = MakeNoticeBase("brandnewfan", "BrandNewFan", "sub", "BrandNewFan subscribed at Tier 1!");
            return new TestEvent(ChatEventType.NewSubscriber, new ChatEventNewSubscriber()
            {
                Id = Model.GetNewId<ChatEventNewSubscriber>(),
                MsgParamCumulativeMonths = 1,
                MsgParamShouldShareStreak = true,
                MsgParamStreakMonths = 1,
                MsgParamSubPlan = SubscriptionPlan.Tier1,
                MsgParamSubPlanName = "Tier 1",
                ResubMessage = "",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreatePrimePaidSubscriber()
        {
            var baseRow = MakeNoticeBase("primeuser", "PrimeUser", "sub", "PrimeUser subscribed with Amazon Prime!");
            return new TestEvent(ChatEventType.PrimePaidSubscriber, new ChatEventPrimePaidSubscriber()
            {
                Id = Model.GetNewId<ChatEventPrimePaidSubscriber>(),
                MsgParamSubPlan = SubscriptionPlan.Prime,
                ResubMessage = "Using my free Prime sub, best use of it all month",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateReSubscriber()
        {
            var baseRow = MakeNoticeBase("loyalfan", "LoyalFan", "resub", "LoyalFan resubscribed for 12 months at Tier 3!");
            return new TestEvent(ChatEventType.ReSubscriber, new ChatEventReSubscriber()
            {
                Id = Model.GetNewId<ChatEventReSubscriber>(),
                MsgParamCumulativeMonths = 12,
                MsgParamShouldShareStreak = true,
                MsgParamStreakMonths = 5,
                MsgParamSubPlan = SubscriptionPlan.Tier3,
                MsgParamSubPlanName = "Tier 3",
                ResubMessage = "Love this channel, best community on Twitch!",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateRitual()
        {
            var baseRow = MakeNoticeBase("newviewer", "NewViewer", "ritual", "NewViewer is new here, say hi!");
            return new TestEvent(ChatEventType.Ritual, new ChatEventRitual()
            {
                Id = Model.GetNewId<ChatEventRitual>(),
                MsgParamRitualName = "new_chatter",
                Message = "is new here, say hi!",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateStandardPayForward()
        {
            var baseRow = MakeNoticeBase("payforwarder", "PayForwarder", "payforward", "PayForwarder is paying forward the gift they got from GifterOne to RecipientTwo");
            return new TestEvent(ChatEventType.StandardPayForward, new ChatEventStandardPayForward()
            {
                Id = Model.GetNewId<ChatEventStandardPayForward>(),
                MsgParamPriorGifterAnonymous = false,
                MsgParamPriorGifterDisplayName = "GifterOne",
                MsgParamPriorGifterId = "2001",
                MsgParamPriorGifterUserName = "gifterone",
                MsgParamRecipientDisplayName = "RecipientTwo",
                MsgParamRecipientId = "3002",
                MsgParamRecipientUserName = "recipienttwo",
                ChatUserNoticeBaseId = baseRow.Id,
            }, baseRow);
        }

        static TestEvent CreateUserBanned()
        {
            return new TestEvent(ChatEventType.UserBanned, new ChatEventUserBanned()
            {
                Id = Model.GetNewId<ChatEventUserBanned>(),
                Channel = "testchannel",
                Username = "banneduser",
                RoomId = "12345678",
                TargetUserId = "4001",
            }, null);
        }

        static TestEvent CreateUserJoined()
        {
            return new TestEvent(ChatEventType.UserJoined, new ChatEventUserJoined()
            {
                Id = Model.GetNewId<ChatEventUserJoined>(),
                Channel = "testchannel",
                Username = "viewer_one",
            }, null);
        }

        static TestEvent CreateUserLeft()
        {
            return new TestEvent(ChatEventType.UserLeft, new ChatEventUserLeft()
            {
                Id = Model.GetNewId<ChatEventUserLeft>(),
                Channel = "testchannel",
                Username = "viewer_one",
            }, null);
        }

        static TestEvent CreateUserTimedout()
        {
            return new TestEvent(ChatEventType.UserTimedout, new ChatEventUserTimedout()
            {
                Id = Model.GetNewId<ChatEventUserTimedout>(),
                Channel = "testchannel",
                TimeoutDuration = TimeSpan.FromMinutes(10),
                Username = "troublemaker",
                TargetUserId = "4002",
            }, null);
        }

        /// <summary>
        /// Shared ChatUserNoticeBase row for user-notice events. Each event gets
        /// its own row so they can all exist in the same test run.
        /// </summary>
        static ChatUserNoticeBase MakeNoticeBase(string username, string displayName, string msgId, string systemMsg)
        {
            return new ChatUserNoticeBase()
            {
                Id = Model.GetNewId<ChatUserNoticeBase>(),
                HexColor = "#9146FF",
                DisplayName = displayName,
                Emotes = "",
                TwitchMessageId = $"test-notice-{username}",
                Username = username,
                MsgId = msgId,
                RoomId = "12345678",
                SystemMsg = systemMsg,
                TmiSent = Now,
                UserFlags = UserDetails.Subscriber | UserDetails.Vip,
                Badges = BadgeSerializer.SerializeBadges(new List<KeyValuePair<string, string>> { new("subscriber", "1"), new("vip", "1") }),
                UserId = $"u-{username}",
                UserType = UserType.Viewer,
            };
        }
    }
}