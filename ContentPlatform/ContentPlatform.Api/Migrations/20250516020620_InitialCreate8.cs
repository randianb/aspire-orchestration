using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelTagEntity",
                schema: "content",
                table: "ChannelTagEntity");

            migrationBuilder.RenameTable(
                name: "ChannelTagEntity",
                schema: "content",
                newName: "ChannelTags",
                newSchema: "content");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelTagEntity_TagCode_ChannelCode",
                schema: "content",
                table: "ChannelTags",
                newName: "IX_ChannelTags_TagCode_ChannelCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelTags",
                schema: "content",
                table: "ChannelTags",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ChannelTagHistories",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelCode = table.Column<string>(type: "text", nullable: false),
                    BodyJson = table.Column<string>(type: "jsonb", nullable: true),
                    SimpleBodyJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelTagHistories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelTagHistories",
                schema: "content");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelTags",
                schema: "content",
                table: "ChannelTags");

            migrationBuilder.RenameTable(
                name: "ChannelTags",
                schema: "content",
                newName: "ChannelTagEntity",
                newSchema: "content");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelTags_TagCode_ChannelCode",
                schema: "content",
                table: "ChannelTagEntity",
                newName: "IX_ChannelTagEntity_TagCode_ChannelCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelTagEntity",
                schema: "content",
                table: "ChannelTagEntity",
                column: "Id");
        }
    }
}
