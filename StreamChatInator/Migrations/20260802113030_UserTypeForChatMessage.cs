using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class UserTypeForChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "UserType",
                table: "ChatEventChatMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "ChatEventChatMessages");
        }
    }
}
