using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class AllEventTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatEventsMessages");

            migrationBuilder.CreateTable(
                name: "ChatEventAnnouncements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamColor = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventAnnouncements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventAnonGiftPaidUpgrades",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamPromoGiftTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamPromoName = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventAnonGiftPaidUpgrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventBitsBadgeTiers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventBitsBadgeTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventChatMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Bits = table.Column<int>(type: "INTEGER", nullable: false),
                    BitsInDollars = table.Column<double>(type: "REAL", nullable: false),
                    CustomRewardId = table.Column<string>(type: "TEXT", nullable: true),
                    EmoteReplacedMessage = table.Column<string>(type: "TEXT", nullable: true),
                    TwitchMessageId = table.Column<string>(type: "TEXT", nullable: false),
                    IsBroadcaster = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFirstMessage = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMe = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSkippingSubMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Noisy = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscribedMonthCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TmiSent = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReplyParentMessageTwitchMessageId = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    HexColor = table.Column<string>(type: "TEXT", nullable: false),
                    UserFlags = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventCommunityPayForwards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamPriorGifterAnonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    MsgParamPriorGifterDisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamPriorGifterId = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamPriorGifterUserName = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventCommunityPayForwards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventCommunitySubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    MsgParamGiftTheme = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamMassGiftCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamOriginId = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamSenderCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamSubPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventCommunitySubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventContinuedGiftedSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamPromoGiftTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamPromoName = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamSenderLogin = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamSenderName = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventContinuedGiftedSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventGiftedSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    MsgParamMonths = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamOriginId = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamRecipientDisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamRecipientId = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamRecipientUserName = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamSenderCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamSubPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamSubPlanName = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamMultiMonthGiftDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventGiftedSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventMessageCleareds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    TargetMessageId = table.Column<string>(type: "TEXT", nullable: false),
                    TmiSent = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventMessageCleareds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventNewSubscribers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamCumulativeMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamShouldShareStreak = table.Column<bool>(type: "INTEGER", nullable: false),
                    MsgParamStreakMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamSubPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamSubPlanName = table.Column<string>(type: "TEXT", nullable: false),
                    ResubMessage = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventNewSubscribers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventPrimePaidSubscribers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamSubPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    ResubMessage = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventPrimePaidSubscribers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventReSubscribers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamCumulativeMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamShouldShareStreak = table.Column<bool>(type: "INTEGER", nullable: false),
                    MsgParamStreakMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamSubPlan = table.Column<int>(type: "INTEGER", nullable: false),
                    MsgParamSubPlanName = table.Column<string>(type: "TEXT", nullable: false),
                    ResubMessage = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventReSubscribers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventRituals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamRitualName = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventRituals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventStandardPayForwards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamPriorGifterAnonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    MsgParamPriorGifterDisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamPriorGifterId = table.Column<long>(type: "INTEGER", nullable: false),
                    MsgParamPriorGifterUserName = table.Column<string>(type: "TEXT", nullable: false),
                    MsgParamRecipientDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    MsgParamRecipientId = table.Column<long>(type: "INTEGER", nullable: true),
                    MsgParamRecipientUserName = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatUserNoticeBaseId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventStandardPayForwards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventUserBanneds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    RoomId = table.Column<string>(type: "TEXT", nullable: false),
                    TargetUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventUserBanneds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventUserJoineds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventUserJoineds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventUserLefts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventUserLefts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventUserTimedouts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", nullable: false),
                    TimeoutDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    TargetUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventUserTimedouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatUserNoticeBases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    HexColor = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Emotes = table.Column<string>(type: "TEXT", nullable: false),
                    TwitchId = table.Column<string>(type: "TEXT", nullable: false),
                    Login = table.Column<string>(type: "TEXT", nullable: false),
                    MsgId = table.Column<string>(type: "TEXT", nullable: false),
                    RoomId = table.Column<string>(type: "TEXT", nullable: false),
                    SystemMsg = table.Column<string>(type: "TEXT", nullable: false),
                    TmiSent = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserFlags = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    UserType = table.Column<byte>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUserNoticeBases", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatEventAnnouncements");

            migrationBuilder.DropTable(
                name: "ChatEventAnonGiftPaidUpgrades");

            migrationBuilder.DropTable(
                name: "ChatEventBitsBadgeTiers");

            migrationBuilder.DropTable(
                name: "ChatEventChatMessages");

            migrationBuilder.DropTable(
                name: "ChatEventCommunityPayForwards");

            migrationBuilder.DropTable(
                name: "ChatEventCommunitySubscriptions");

            migrationBuilder.DropTable(
                name: "ChatEventContinuedGiftedSubscriptions");

            migrationBuilder.DropTable(
                name: "ChatEventGiftedSubscriptions");

            migrationBuilder.DropTable(
                name: "ChatEventMessageCleareds");

            migrationBuilder.DropTable(
                name: "ChatEventNewSubscribers");

            migrationBuilder.DropTable(
                name: "ChatEventPrimePaidSubscribers");

            migrationBuilder.DropTable(
                name: "ChatEventReSubscribers");

            migrationBuilder.DropTable(
                name: "ChatEventRituals");

            migrationBuilder.DropTable(
                name: "ChatEventStandardPayForwards");

            migrationBuilder.DropTable(
                name: "ChatEventUserBanneds");

            migrationBuilder.DropTable(
                name: "ChatEventUserJoineds");

            migrationBuilder.DropTable(
                name: "ChatEventUserLefts");

            migrationBuilder.DropTable(
                name: "ChatEventUserTimedouts");

            migrationBuilder.DropTable(
                name: "ChatUserNoticeBases");

            migrationBuilder.CreateTable(
                name: "ChatEventsMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Bits = table.Column<int>(type: "INTEGER", nullable: false),
                    BitsInDollars = table.Column<double>(type: "REAL", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomRewardId = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    EmoteReplacedMessage = table.Column<string>(type: "TEXT", nullable: true),
                    HexColor = table.Column<string>(type: "TEXT", nullable: false),
                    IsBroadcaster = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFirstMessage = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMe = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSkippingSubMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Noisy = table.Column<int>(type: "INTEGER", nullable: false),
                    ReplyParentMessageTwitchMessageId = table.Column<string>(type: "TEXT", nullable: true),
                    SubscribedMonthCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TmiSent = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TwitchMessageId = table.Column<string>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserFlags = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventsMessages", x => x.Id);
                });
        }
    }
}
