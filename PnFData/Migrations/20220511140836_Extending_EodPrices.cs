using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Extending_EodPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AdjustedClose",
                table: "EodPrices",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DividendAmount",
                table: "EodPrices",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SplitCoefficient",
                table: "EodPrices",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustedClose",
                table: "EodPrices");

            migrationBuilder.DropColumn(
                name: "DividendAmount",
                table: "EodPrices");

            migrationBuilder.DropColumn(
                name: "SplitCoefficient",
                table: "EodPrices");
        }
    }
}
