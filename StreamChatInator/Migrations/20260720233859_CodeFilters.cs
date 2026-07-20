using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class CodeFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilterType",
                table: "EventFilters");

            migrationBuilder.RenameColumn(
                name: "ConditionsJson",
                table: "EventFilters",
                newName: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "EventFilters",
                newName: "ConditionsJson");

            migrationBuilder.AddColumn<int>(
                name: "FilterType",
                table: "EventFilters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
