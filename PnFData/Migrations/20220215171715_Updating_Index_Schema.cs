using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Updating_Index_Schema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexShares");

            migrationBuilder.DropTable(
                name: "IndexDates");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Indices");

            migrationBuilder.AddColumn<string>(
                name: "ExchangeCode",
                table: "Indices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExchangeSubCode",
                table: "Indices",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IndexValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndexId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Contributors = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndexValues_Indices_IndexId",
                        column: x => x.IndexId,
                        principalTable: "Indices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexValues_Day",
                table: "IndexValues",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_IndexValues_IndexId",
                table: "IndexValues",
                column: "IndexId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexValues");

            migrationBuilder.DropColumn(
                name: "ExchangeCode",
                table: "Indices");

            migrationBuilder.DropColumn(
                name: "ExchangeSubCode",
                table: "Indices");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Indices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "IndexDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndexId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "getdate()"),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndexDates_Indices_IndexId",
                        column: x => x.IndexId,
                        principalTable: "Indices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndexShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndexId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShareId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "getdate()"),
                    Factor = table.Column<double>(type: "float", nullable: false),
                    IndexDateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndexShares_IndexDates_IndexDateId",
                        column: x => x.IndexDateId,
                        principalTable: "IndexDates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndexShares_Indices_IndexId",
                        column: x => x.IndexId,
                        principalTable: "Indices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndexShares_Shares_ShareId",
                        column: x => x.ShareId,
                        principalTable: "Shares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexDates_Day",
                table: "IndexDates",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_IndexDates_IndexId",
                table: "IndexDates",
                column: "IndexId");

            migrationBuilder.CreateIndex(
                name: "IX_IndexShares_IndexDateId",
                table: "IndexShares",
                column: "IndexDateId");

            migrationBuilder.CreateIndex(
                name: "IX_IndexShares_IndexId",
                table: "IndexShares",
                column: "IndexId");

            migrationBuilder.CreateIndex(
                name: "IX_IndexShares_ShareId",
                table: "IndexShares",
                column: "ShareId");
        }
    }
}
