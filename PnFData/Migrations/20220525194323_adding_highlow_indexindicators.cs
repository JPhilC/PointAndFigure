using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class adding_highlow_indexindicators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HighLowIndexRising",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HighLowIndexfalling",
                table: "IndexIndicators",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighLowIndexRising",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "HighLowIndexfalling",
                table: "IndexIndicators");
        }
    }
}
