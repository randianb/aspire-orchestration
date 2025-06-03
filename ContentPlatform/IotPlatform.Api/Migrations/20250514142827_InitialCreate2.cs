using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverCode",
                schema: "content",
                table: "Tags",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverCode",
                schema: "content",
                table: "Tags");
        }
    }
}
