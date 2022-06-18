using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_ChartPriceScaling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HighLowIndexfalling",
                table: "IndexIndicators",
                newName: "HighLowIndexFalling");

            migrationBuilder.AddColumn<double>(
                name: "BaseValue",
                table: "PnFCharts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceScale",
                table: "PnFCharts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseValue",
                table: "PnFCharts");

            migrationBuilder.DropColumn(
                name: "PriceScale",
                table: "PnFCharts");

            migrationBuilder.RenameColumn(
                name: "HighLowIndexFalling",
                table: "IndexIndicators",
                newName: "HighLowIndexfalling");
        }
    }
}
