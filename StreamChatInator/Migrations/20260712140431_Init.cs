using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ChatEventType = table.Column<int>(type: "INTEGER", nullable: false),
                    EventId = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventsMessages",
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
                    TmiSent = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ReplyParentMessageTwitchMessageId = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventsMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettingValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingValues", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatEvents");

            migrationBuilder.DropTable(
                name: "ChatEventsMessages");

            migrationBuilder.DropTable(
                name: "SettingValues");
        }
    }
}
