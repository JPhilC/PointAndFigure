using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_BullishPercentDoubles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BullishPercentDoubleBottom",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BullishPercentDoubleTop",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BullishPercentDoubleBottom",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "BullishPercentDoubleTop",
                table: "IndexIndicators");
        }
    }
}
