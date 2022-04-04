using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Fixing_Index_Indicator_Typo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PercentRsFallingFalling",
                table: "IndexIndicators",
                newName: "PercentRsRisingFalling");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PercentRsRisingFalling",
                table: "IndexIndicators",
                newName: "PercentRsFallingFalling");
        }
    }
}
