using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamChatInator.Migrations
{
    /// <inheritdoc />
    public partial class Consistency_FixFieldNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TwitchId",
                table: "ChatUserNoticeBases",
                newName: "TwitchMessageId");

            migrationBuilder.RenameColumn(
                name: "Login",
                table: "ChatUserNoticeBases",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "TargetMessageId",
                table: "ChatEventMessageCleareds",
                newName: "TargetTwitchMessageId");

            migrationBuilder.RenameColumn(
                name: "MsgParamSenderLogin",
                table: "ChatEventContinuedGiftedSubscriptions",
                newName: "MsgParamSenderUsername");

            migrationBuilder.AlterColumn<string>(
                name: "MsgParamRecipientId",
                table: "ChatEventStandardPayForwards",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MsgParamPriorGifterId",
                table: "ChatEventStandardPayForwards",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "MsgParamMonths",
                table: "ChatEventGiftedSubscriptions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TwitchMessageId",
                table: "ChatUserNoticeBases",
                newName: "TwitchId");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "ChatUserNoticeBases",
                newName: "Login");

            migrationBuilder.RenameColumn(
                name: "TargetTwitchMessageId",
                table: "ChatEventMessageCleareds",
                newName: "TargetMessageId");

            migrationBuilder.RenameColumn(
                name: "MsgParamSenderUsername",
                table: "ChatEventContinuedGiftedSubscriptions",
                newName: "MsgParamSenderLogin");

            migrationBuilder.AlterColumn<long>(
                name: "MsgParamRecipientId",
                table: "ChatEventStandardPayForwards",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MsgParamPriorGifterId",
                table: "ChatEventStandardPayForwards",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "MsgParamMonths",
                table: "ChatEventGiftedSubscriptions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
