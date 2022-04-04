using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class UpdatingIndexValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BullishPercent",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAboveEma10",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAboveEma30",
                table: "IndexIndicators");

            migrationBuilder.AddColumn<double>(
                name: "BullishPercent",
                table: "IndexValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentAboveEma10",
                table: "IndexValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentAboveEma30",
                table: "IndexValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentPositiveTrend",
                table: "IndexValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentRsBuy",
                table: "IndexValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentRsRising",
                table: "IndexValues",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BullishPercent",
                table: "IndexValues");

            migrationBuilder.DropColumn(
                name: "PercentAboveEma10",
                table: "IndexValues");

            migrationBuilder.DropColumn(
                name: "PercentAboveEma30",
                table: "IndexValues");

            migrationBuilder.DropColumn(
                name: "PercentPositiveTrend",
                table: "IndexValues");

            migrationBuilder.DropColumn(
                name: "PercentRsBuy",
                table: "IndexValues");

            migrationBuilder.DropColumn(
                name: "PercentRsRising",
                table: "IndexValues");

            migrationBuilder.AddColumn<double>(
                name: "BullishPercent",
                table: "IndexIndicators",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentAboveEma10",
                table: "IndexIndicators",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentAboveEma30",
                table: "IndexIndicators",
                type: "float",
                nullable: true);
        }
    }
}
