using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_Index_Sector : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Indices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Indices");
        }
    }
}
