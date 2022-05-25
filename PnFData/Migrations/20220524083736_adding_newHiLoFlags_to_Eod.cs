using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class adding_newHiLoFlags_to_Eod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "New52WeekHigh",
                table: "EodPrices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "New52WeekLow",
                table: "EodPrices",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "New52WeekHigh",
                table: "EodPrices");

            migrationBuilder.DropColumn(
                name: "New52WeekLow",
                table: "EodPrices");
        }
    }
}
