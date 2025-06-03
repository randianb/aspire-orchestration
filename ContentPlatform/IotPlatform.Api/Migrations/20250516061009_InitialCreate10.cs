using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SenderEntity",
                schema: "content",
                table: "SenderEntity");

            migrationBuilder.RenameTable(
                name: "SenderEntity",
                schema: "content",
                newName: "Senders",
                newSchema: "content");

            migrationBuilder.RenameIndex(
                name: "IX_SenderEntity_SenderCode",
                schema: "content",
                table: "Senders",
                newName: "IX_Senders_SenderCode");

            migrationBuilder.AlterColumn<string>(
                name: "MachineCode",
                schema: "content",
                table: "Senders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DriverCode",
                schema: "content",
                table: "Senders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Desc",
                schema: "content",
                table: "Senders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "OptionsJson",
                schema: "content",
                table: "Senders",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Senders",
                schema: "content",
                table: "Senders",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Senders",
                schema: "content",
                table: "Senders");

            migrationBuilder.DropColumn(
                name: "OptionsJson",
                schema: "content",
                table: "Senders");

            migrationBuilder.RenameTable(
                name: "Senders",
                schema: "content",
                newName: "SenderEntity",
                newSchema: "content");

            migrationBuilder.RenameIndex(
                name: "IX_Senders_SenderCode",
                schema: "content",
                table: "SenderEntity",
                newName: "IX_SenderEntity_SenderCode");

            migrationBuilder.AlterColumn<string>(
                name: "MachineCode",
                schema: "content",
                table: "SenderEntity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DriverCode",
                schema: "content",
                table: "SenderEntity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Desc",
                schema: "content",
                table: "SenderEntity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SenderEntity",
                schema: "content",
                table: "SenderEntity",
                column: "Id");
        }
    }
}
