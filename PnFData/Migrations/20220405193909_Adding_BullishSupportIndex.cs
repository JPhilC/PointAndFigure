using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_BullishSupportIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BullSupportIndex",
                table: "PnFColumns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BullSupportIndex",
                table: "PnFColumns");
        }
    }
}
