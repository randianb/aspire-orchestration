using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverCode",
                schema: "content",
                table: "ChannelTags",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFull",
                schema: "content",
                table: "Channels",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverCode",
                schema: "content",
                table: "ChannelTags");

            migrationBuilder.DropColumn(
                name: "IsFull",
                schema: "content",
                table: "Channels");
        }
    }
}
