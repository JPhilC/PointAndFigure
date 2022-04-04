using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Extending_Indicators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DoubleBottom",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoubleTop",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Falling",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PeerRsBuy",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PeerRsFalling",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PeerRsRising",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PeerRsSell",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Rising",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RsBuy",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RsFalling",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RsRising",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RsSell",
                table: "ShareIndicators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TripleBottom",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TripleTop",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IndexIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rising = table.Column<bool>(type: "bit", nullable: true),
                    Buy = table.Column<bool>(type: "bit", nullable: true),
                    RsRising = table.Column<bool>(type: "bit", nullable: true),
                    RsBuy = table.Column<bool>(type: "bit", nullable: true),
                    Falling = table.Column<bool>(type: "bit", nullable: true),
                    Sell = table.Column<bool>(type: "bit", nullable: true),
                    RsFalling = table.Column<bool>(type: "bit", nullable: true),
                    RsSell = table.Column<bool>(type: "bit", nullable: true),
                    BullishPercent = table.Column<double>(type: "float", nullable: true),
                    IndexId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndexIndicators_Indices_IndexId",
                        column: x => x.IndexId,
                        principalTable: "Indices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexIndicators_IndexId",
                table: "IndexIndicators",
                column: "IndexId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "DoubleBottom",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "DoubleTop",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "Falling",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "PeerRsBuy",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "PeerRsFalling",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "PeerRsRising",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "PeerRsSell",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "Rising",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "RsBuy",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "RsFalling",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "RsRising",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "RsSell",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "TripleBottom",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "TripleTop",
                table: "ShareIndicators");
        }
    }
}
