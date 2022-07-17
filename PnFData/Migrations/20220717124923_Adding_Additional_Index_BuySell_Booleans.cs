using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_Additional_Index_BuySell_Booleans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HighLowIndexBuy",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HighLowIndexSell",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove10EmaBuy",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove10EmaSell",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove30EmaBuy",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove30EmaSell",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentPositiveTrendBuy",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentPositiveTrendSell",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRSBuyBuy",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRSBuySell",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRsRisingBuy",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRsRisingSell",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighLowIndexBuy",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "HighLowIndexSell",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove10EmaBuy",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove10EmaSell",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove30EmaBuy",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove30EmaSell",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentPositiveTrendBuy",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentPositiveTrendSell",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRSBuyBuy",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRSBuySell",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRsRisingBuy",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRsRisingSell",
                table: "IndexIndicators");
        }
    }
}
