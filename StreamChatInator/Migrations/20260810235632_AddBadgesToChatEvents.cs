using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgesToChatEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Badges",
                table: "ChatUserNoticeBases",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Badges",
                table: "ChatEventChatMessages",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Badges",
                table: "ChatUserNoticeBases");

            migrationBuilder.DropColumn(
                name: "Badges",
                table: "ChatEventChatMessages");
        }
    }
}
