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

        public static IEnumerable<(ChatEventType Type, Model Data, Model? SubData)> CreateAll()
        {
            yield return (ChatEventType.ChatMessage, CreateChatMessage(), null);

            var announcement = CreateAnnouncement();
            yield return (ChatEventType.Announcement, announcement.Item1, announcement.Item2);

            var anonUpgrade = CreateAnonGiftPaidUpgrade();
            yield return (ChatEventType.AnonGiftPaidUpgrade, anonUpgrade.Item1, anonUpgrade.Item2);

            var bitsBadge = CreateBitsBadgeTier();
            yield return (ChatEventType.BitsBadgeTier, bitsBadge.Item1, bitsBadge.Item2);

            var communityPayForward = CreateCommunityPayForward();
            yield return (ChatEventType.CommunityPayForward, communityPayForward.Item1, communityPayForward.Item2);

            var communitySub = CreateCommunitySubscription();
            yield return (ChatEventType.CommunitySubscription, communitySub.Item1, communitySub.Item2);

            var continuedGift = CreateContinuedGiftedSubscription();
            yield return (ChatEventType.ContinuedGiftedSubscription, continuedGift.Item1, continuedGift.Item2);

            var gift = CreateGiftedSubscription();
            yield return (ChatEventType.GiftedSubscription, gift.Item1, gift.Item2);

            yield return (ChatEventType.MessageCleared, CreateMessageCleared(), null);

            var newSub = CreateNewSubscriber();
            yield return (ChatEventType.NewSubscriber, newSub.Item1, newSub.Item2);

            var primeSub = CreatePrimePaidSubscriber();
            yield return (ChatEventType.PrimePaidSubscriber, primeSub.Item1, primeSub.Item2);

            var resub = CreateReSubscriber();
            yield return (ChatEventType.ReSubscriber, resub.Item1, resub.Item2);

            var ritual = CreateRitual();
            yield return (ChatEventType.Ritual, ritual.Item1, ritual.Item2);

            var payForward = CreateStandardPayForward();
            yield return (ChatEventType.StandardPayForward, payForward.Item1, payForward.Item2);

            yield return (ChatEventType.UserBanned, CreateUserBanned(), null);
            yield return (ChatEventType.UserJoined, CreateUserJoined(), null);
            yield return (ChatEventType.UserLeft, CreateUserLeft(), null);
            yield return (ChatEventType.UserTimedout, CreateUserTimedout(), null);
        }

        static ChatEventChatMessage CreateChatMessage()
        {
            return new ChatEventChatMessage()
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
                Badges = Util.SerializeBadges(new List<KeyValuePair<string, string>> { new("moderator", "1"), new("subscriber", "6") }),
                UserFlags = UserDetails.Moderator | UserDetails.Subscriber | UserDetails.Vip,
                UserType = UserType.Moderator,
            };
        }

        static (ChatEventAnnouncement, ChatUserNoticeBase) CreateAnnouncement()
        {
            var baseRow = MakeNoticeBase("announcer", "Announcer", "announcement", "announcer made an announcement");
            var detail = new ChatEventAnnouncement()
            {
                Id = Model.GetNewId<ChatEventAnnouncement>(),
                MsgParamColor = "PRIMARY",
                Message = "Check out the new schedule - streams every Tuesday and Friday!",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventAnonGiftPaidUpgrade, ChatUserNoticeBase) CreateAnonGiftPaidUpgrade()
        {
            var baseRow = MakeNoticeBase("anonupgrader", "Anonymous", "anongiftpaidupgrade", "an anonymous user upgraded their gifted sub");
            var detail = new ChatEventAnonGiftPaidUpgrade()
            {
                Id = Model.GetNewId<ChatEventAnonGiftPaidUpgrade>(),
                MsgParamPromoGiftTotal = 5,
                MsgParamPromoName = "Subtember 2026",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventBitsBadgeTier, ChatUserNoticeBase) CreateBitsBadgeTier()
        {
            var baseRow = MakeNoticeBase("bitbadder", "BitBadder", "bitsbadgetier", "BitBadder earned the 1000-bit badge");
            var detail = new ChatEventBitsBadgeTier()
            {
                Id = Model.GetNewId<ChatEventBitsBadgeTier>(),
                MsgParamThreshold = 1000,
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventCommunityPayForward, ChatUserNoticeBase) CreateCommunityPayForward()
        {
            var baseRow = MakeNoticeBase("payforwarder", "PayForwarder", "communitypayforward", "PayForwarder is paying forward a gift from GifterOne to the community");
            var detail = new ChatEventCommunityPayForward()
            {
                Id = Model.GetNewId<ChatEventCommunityPayForward>(),
                MsgParamPriorGifterAnonymous = false,
                MsgParamPriorGifterDisplayName = "GifterOne",
                MsgParamPriorGifterId = "2001",
                MsgParamPriorGifterUserName = "gifterone",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventCommunitySubscription, ChatUserNoticeBase) CreateCommunitySubscription()
        {
            var baseRow = MakeNoticeBase("massgifter", "MassGifter", "subgift", "MassGifter gifted 10 subs to the community");
            var detail = new ChatEventCommunitySubscription()
            {
                Id = Model.GetNewId<ChatEventCommunitySubscription>(),
                IsAnonymous = false,
                MsgParamGiftTheme = "TwitchCon",
                MsgParamMassGiftCount = 10,
                MsgParamOriginId = "test-origin-1",
                MsgParamSenderCount = 25,
                MsgParamSubPlan = SubscriptionPlan.Tier1,
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventContinuedGiftedSubscription, ChatUserNoticeBase) CreateContinuedGiftedSubscription()
        {
            var baseRow = MakeNoticeBase("continued", "ContinuedGifter", "subgift", "ContinuedGifter's gifted sub to RecipientOne was renewed");
            var detail = new ChatEventContinuedGiftedSubscription()
            {
                Id = Model.GetNewId<ChatEventContinuedGiftedSubscription>(),
                MsgParamPromoGiftTotal = 3,
                MsgParamPromoName = "Subtember 2026",
                MsgParamSenderUsername = "gifterone",
                MsgParamSenderName = "GifterOne",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventGiftedSubscription, ChatUserNoticeBase) CreateGiftedSubscription()
        {
            var baseRow = MakeNoticeBase("recipientone", "RecipientOne", "subgift", "GifterOne gifted a Tier 1 sub to RecipientOne!");
            var detail = new ChatEventGiftedSubscription()
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
            };
            return (detail, baseRow);
        }

        static ChatEventMessageCleared CreateMessageCleared()
        {
            return new ChatEventMessageCleared()
            {
                Id = Model.GetNewId<ChatEventMessageCleared>(),
                Channel = "testchannel",
                Message = "This message was deleted by a moderator",
                TargetTwitchMessageId = "test-msg-2",
                TmiSent = Now,
            };
        }

        static (ChatEventNewSubscriber, ChatUserNoticeBase) CreateNewSubscriber()
        {
            var baseRow = MakeNoticeBase("brandnewfan", "BrandNewFan", "sub", "BrandNewFan subscribed at Tier 1!");
            var detail = new ChatEventNewSubscriber()
            {
                Id = Model.GetNewId<ChatEventNewSubscriber>(),
                MsgParamCumulativeMonths = 1,
                MsgParamShouldShareStreak = true,
                MsgParamStreakMonths = 1,
                MsgParamSubPlan = SubscriptionPlan.Tier1,
                MsgParamSubPlanName = "Tier 1",
                ResubMessage = "",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventPrimePaidSubscriber, ChatUserNoticeBase) CreatePrimePaidSubscriber()
        {
            var baseRow = MakeNoticeBase("primeuser", "PrimeUser", "sub", "PrimeUser subscribed with Amazon Prime!");
            var detail = new ChatEventPrimePaidSubscriber()
            {
                Id = Model.GetNewId<ChatEventPrimePaidSubscriber>(),
                MsgParamSubPlan = SubscriptionPlan.Prime,
                ResubMessage = "Using my free Prime sub, best use of it all month",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventReSubscriber, ChatUserNoticeBase) CreateReSubscriber()
        {
            var baseRow = MakeNoticeBase("loyalfan", "LoyalFan", "resub", "LoyalFan resubscribed for 12 months at Tier 3!");
            var detail = new ChatEventReSubscriber()
            {
                Id = Model.GetNewId<ChatEventReSubscriber>(),
                MsgParamCumulativeMonths = 12,
                MsgParamShouldShareStreak = true,
                MsgParamStreakMonths = 5,
                MsgParamSubPlan = SubscriptionPlan.Tier3,
                MsgParamSubPlanName = "Tier 3",
                ResubMessage = "Love this channel, best community on Twitch!",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventRitual, ChatUserNoticeBase) CreateRitual()
        {
            var baseRow = MakeNoticeBase("newviewer", "NewViewer", "ritual", "NewViewer is new here, say hi!");
            var detail = new ChatEventRitual()
            {
                Id = Model.GetNewId<ChatEventRitual>(),
                MsgParamRitualName = "new_chatter",
                Message = "is new here, say hi!",
                ChatUserNoticeBaseId = baseRow.Id,
            };
            return (detail, baseRow);
        }

        static (ChatEventStandardPayForward, ChatUserNoticeBase) CreateStandardPayForward()
        {
            var baseRow = MakeNoticeBase("payforwarder", "PayForwarder", "payforward", "PayForwarder is paying forward the gift they got from GifterOne to RecipientTwo");
            var detail = new ChatEventStandardPayForward()
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
            };
            return (detail, baseRow);
        }

        static ChatEventUserBanned CreateUserBanned()
        {
            return new ChatEventUserBanned()
            {
                Id = Model.GetNewId<ChatEventUserBanned>(),
                Channel = "testchannel",
                Username = "banneduser",
                RoomId = "12345678",
                TargetUserId = "4001",
            };
        }

        static ChatEventUserJoined CreateUserJoined()
        {
            return new ChatEventUserJoined()
            {
                Id = Model.GetNewId<ChatEventUserJoined>(),
                Channel = "testchannel",
                Username = "viewer_one",
            };
        }

        static ChatEventUserLeft CreateUserLeft()
        {
            return new ChatEventUserLeft()
            {
                Id = Model.GetNewId<ChatEventUserLeft>(),
                Channel = "testchannel",
                Username = "viewer_one",
            };
        }

        static ChatEventUserTimedout CreateUserTimedout()
        {
            return new ChatEventUserTimedout()
            {
                Id = Model.GetNewId<ChatEventUserTimedout>(),
                Channel = "testchannel",
                TimeoutDuration = TimeSpan.FromMinutes(10),
                Username = "troublemaker",
                TargetUserId = "4002",
            };
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
                Badges = Util.SerializeBadges(new List<KeyValuePair<string, string>> { new("subscriber", "1"), new("vip", "1") }),
                UserId = $"u-{username}",
                UserType = UserType.Viewer,
            };
        }
    }
}
