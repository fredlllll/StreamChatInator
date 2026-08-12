using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class RenameEventFiltersToChatEventFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventFilters",
                table: "EventFilters");

            migrationBuilder.RenameTable(
                name: "EventFilters",
                newName: "ChatEventFilters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatEventFilters",
                table: "ChatEventFilters",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatEventFilters",
                table: "ChatEventFilters");

            migrationBuilder.RenameTable(
                name: "ChatEventFilters",
                newName: "EventFilters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventFilters",
                table: "EventFilters",
                column: "Id");
        }
    }
}
