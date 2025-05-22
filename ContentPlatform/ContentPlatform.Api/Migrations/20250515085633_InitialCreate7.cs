using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChannelTagEntity_ChannelCode",
                schema: "content",
                table: "ChannelTagEntity");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTagEntity_TagCode_ChannelCode",
                schema: "content",
                table: "ChannelTagEntity",
                columns: new[] { "TagCode", "ChannelCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChannelTagEntity_TagCode_ChannelCode",
                schema: "content",
                table: "ChannelTagEntity");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTagEntity_ChannelCode",
                schema: "content",
                table: "ChannelTagEntity",
                column: "ChannelCode",
                unique: true);
        }
    }
}
