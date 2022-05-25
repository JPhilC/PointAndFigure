using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_52weekhighandlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "High52Week",
                table: "EodPrices",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Low52Week",
                table: "EodPrices",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "High52Week",
                table: "EodPrices");

            migrationBuilder.DropColumn(
                name: "Low52Week",
                table: "EodPrices");
        }
    }
}
