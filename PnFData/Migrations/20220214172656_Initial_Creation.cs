using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Initial_Creation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Indices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tidm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExchangeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SharesInIssueMillions = table.Column<double>(type: "float", nullable: false),
                    MarketCapMillions = table.Column<double>(type: "float", nullable: false),
                    SuperSector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricesCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ShareDataSource = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    ShareDataSourceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastEodDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EodError = table.Column<bool>(type: "bit", nullable: false),
                    ExchangeSubCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndexDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndexId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "EodPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Open = table.Column<double>(type: "float", nullable: false),
                    High = table.Column<double>(type: "float", nullable: false),
                    Low = table.Column<double>(type: "float", nullable: false),
                    Close = table.Column<double>(type: "float", nullable: false),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    ShareId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EodPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EodPrices_Shares_ShareId",
                        column: x => x.ShareId,
                        principalTable: "Shares",
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
                    Factor = table.Column<double>(type: "float", nullable: false),
                    IndexDateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "IX_EodPrices_Day",
                table: "EodPrices",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_EodPrices_ShareId",
                table: "EodPrices",
                column: "ShareId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Shares_ShareDataSource_ShareDataSourceId",
                table: "Shares",
                columns: new[] { "ShareDataSource", "ShareDataSourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shares_Tidm",
                table: "Shares",
                column: "Tidm",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EodPrices");

            migrationBuilder.DropTable(
                name: "IndexShares");

            migrationBuilder.DropTable(
                name: "IndexDates");

            migrationBuilder.DropTable(
                name: "Shares");

            migrationBuilder.DropTable(
                name: "Indices");
        }
    }
}
