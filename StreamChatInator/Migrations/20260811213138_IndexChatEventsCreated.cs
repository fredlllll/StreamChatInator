using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class IndexChatEventsCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChatEvents_Created",
                table: "ChatEvents",
                column: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatEvents_Created",
                table: "ChatEvents");
        }
    }
}
