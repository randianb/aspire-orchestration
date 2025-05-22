using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChannelTagCode",
                schema: "content",
                table: "ChannelTagEntity",
                newName: "ChannelCode");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelTagEntity_ChannelTagCode",
                schema: "content",
                table: "ChannelTagEntity",
                newName: "IX_ChannelTagEntity_ChannelCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChannelCode",
                schema: "content",
                table: "ChannelTagEntity",
                newName: "ChannelTagCode");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelTagEntity_ChannelCode",
                schema: "content",
                table: "ChannelTagEntity",
                newName: "IX_ChannelTagEntity_ChannelTagCode");
        }
    }
}
