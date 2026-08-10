using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeJsField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeJs",
                table: "EventFilters",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeJs",
                table: "EventFilters");
        }
    }
}
