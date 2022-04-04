using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Updating_Index_Indicators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BullishPercentFalling",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BullishPercentRising",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove10EmaFalling",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove10EmaRising",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove30EmaFalling",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentAbove30EmaRising",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentPositiveTrendFalling",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentPositiveTrendRising",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRSBuyFalling",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRSBuyRising",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRsFallingFalling",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PercentRsRisingRising",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BullishPercentFalling",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "BullishPercentRising",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove10EmaFalling",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove10EmaRising",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove30EmaFalling",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAbove30EmaRising",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentPositiveTrendFalling",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentPositiveTrendRising",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRSBuyFalling",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRSBuyRising",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRsFallingFalling",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentRsRisingRising",
                table: "IndexIndicators");
        }
    }
}
