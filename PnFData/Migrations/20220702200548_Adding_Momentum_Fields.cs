using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_Momentum_Fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Ema1",
                table: "ShareIndicators",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Ema5",
                table: "ShareIndicators",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MomentumFalling",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MomentumRising",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WeeklyMomentum",
                table: "ShareIndicators",
                type: "float",
                nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "Portfolios",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
            //        Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
            //        Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "getdate()"),
            //        UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Portfolios", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PortfolioShares",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
            //        PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ShareId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Holding = table.Column<double>(type: "float", nullable: false),
            //        Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "getdate()"),
            //        UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PortfolioShares", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_PortfolioShares_Portfolios_PortfolioId",
            //            column: x => x.PortfolioId,
            //            principalTable: "Portfolios",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_PortfolioShares_Shares_ShareId",
            //            column: x => x.ShareId,
            //            principalTable: "Shares",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Portfolios_Name",
            //    table: "Portfolios",
            //    column: "Name",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_PortfolioShares_PortfolioId",
            //    table: "PortfolioShares",
            //    column: "PortfolioId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_PortfolioShares_ShareId",
            //    table: "PortfolioShares",
            //    column: "ShareId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "PortfolioShares");

            //migrationBuilder.DropTable(
            //    name: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Ema1",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "Ema5",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "MomentumFalling",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "MomentumRising",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "WeeklyMomentum",
                table: "ShareIndicators");
        }
    }
}
