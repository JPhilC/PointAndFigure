using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_ShowBullishSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowBullishSupport",
                table: "PnFColumns",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowBullishSupport",
                table: "PnFColumns");
        }
    }
}
