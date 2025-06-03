using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderCode",
                schema: "content",
                table: "Channels");

            migrationBuilder.AddColumn<List<string>>(
                name: "SenderCodes",
                schema: "content",
                table: "Channels",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderCodes",
                schema: "content",
                table: "Channels");

            migrationBuilder.AddColumn<string>(
                name: "SenderCode",
                schema: "content",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
