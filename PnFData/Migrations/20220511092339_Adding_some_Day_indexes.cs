using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_some_Day_indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShareRSIValues_Day",
                table: "ShareRSIValues",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_IndexRSIValues_Day",
                table: "IndexRSIValues",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_IndexIndicators_Day",
                table: "IndexIndicators",
                column: "Day");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShareRSIValues_Day",
                table: "ShareRSIValues");

            migrationBuilder.DropIndex(
                name: "IX_IndexRSIValues_Day",
                table: "IndexRSIValues");

            migrationBuilder.DropIndex(
                name: "IX_IndexIndicators_Day",
                table: "IndexIndicators");
        }
    }
}
