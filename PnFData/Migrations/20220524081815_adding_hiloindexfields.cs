using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class adding_hiloindexfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HighLowEma10",
                table: "IndexValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentHighLow",
                table: "IndexValues",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighLowEma10",
                table: "IndexValues");

            migrationBuilder.DropColumn(
                name: "PercentHighLow",
                table: "IndexValues");
        }
    }
}
