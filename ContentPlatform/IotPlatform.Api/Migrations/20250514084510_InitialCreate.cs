using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelTagEntity",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelTagCode = table.Column<string>(type: "text", nullable: false),
                    GroupCode = table.Column<string>(type: "text", nullable: false),
                    EquipCode = table.Column<string>(type: "text", nullable: false),
                    TagCode = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValueJson = table.Column<string>(type: "jsonb", nullable: true),
                    LastValueJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelTagEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelCode = table.Column<string>(type: "text", nullable: false),
                    IsSchedule = table.Column<bool>(type: "boolean", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    SenderCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverCode = table.Column<string>(type: "text", nullable: false),
                    DriverType = table.Column<int>(type: "integer", nullable: false),
                    MachineCode = table.Column<string>(type: "text", nullable: false),
                    ServerName = table.Column<string>(type: "text", nullable: false),
                    ServerUrl = table.Column<string>(type: "text", nullable: false),
                    HasIdentity = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PassWord = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipEntity",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipCode = table.Column<string>(type: "text", nullable: false),
                    EquipName = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupEntity",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipCode = table.Column<string>(type: "text", nullable: false),
                    GroupCode = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineEntity",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineCode = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SenderEntity",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderCode = table.Column<string>(type: "text", nullable: false),
                    MachineCode = table.Column<string>(type: "text", nullable: false),
                    DriverCode = table.Column<string>(type: "text", nullable: false),
                    SenderType = table.Column<int>(type: "integer", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupCode = table.Column<string>(type: "text", nullable: false),
                    EquipCode = table.Column<string>(type: "text", nullable: false),
                    TagCode = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Scaling = table.Column<double>(type: "double precision", nullable: false),
                    Shifting = table.Column<double>(type: "double precision", nullable: false),
                    ValueJson = table.Column<string>(type: "jsonb", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastValueJson = table.Column<string>(type: "jsonb", nullable: true),
                    LastUpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTagEntity_ChannelTagCode",
                schema: "content",
                table: "ChannelTagEntity",
                column: "ChannelTagCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ChannelCode",
                schema: "content",
                table: "Channels",
                column: "ChannelCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_DriverCode",
                schema: "content",
                table: "Drivers",
                column: "DriverCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipEntity_EquipCode",
                schema: "content",
                table: "EquipEntity",
                column: "EquipCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupEntity_EquipCode_GroupCode",
                schema: "content",
                table: "GroupEntity",
                columns: new[] { "EquipCode", "GroupCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineEntity_MachineCode",
                schema: "content",
                table: "MachineEntity",
                column: "MachineCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SenderEntity_SenderCode",
                schema: "content",
                table: "SenderEntity",
                column: "SenderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_EquipCode_GroupCode_TagCode",
                schema: "content",
                table: "Tags",
                columns: new[] { "EquipCode", "GroupCode", "TagCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelTagEntity",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Channels",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Drivers",
                schema: "content");

            migrationBuilder.DropTable(
                name: "EquipEntity",
                schema: "content");

            migrationBuilder.DropTable(
                name: "GroupEntity",
                schema: "content");

            migrationBuilder.DropTable(
                name: "MachineEntity",
                schema: "content");

            migrationBuilder.DropTable(
                name: "SenderEntity",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "content");
        }
    }
}
