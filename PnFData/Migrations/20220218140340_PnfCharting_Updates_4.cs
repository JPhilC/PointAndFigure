using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class PnfCharting_Updates_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShareCharts_PnFCharts_ShareId",
                table: "ShareCharts");

            migrationBuilder.DropIndex(
                name: "IX_ShareCharts_ShareId",
                table: "ShareCharts");

            migrationBuilder.CreateIndex(
                name: "IX_ShareCharts_ChartId",
                table: "ShareCharts",
                column: "ChartId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShareCharts_ShareId",
                table: "ShareCharts",
                column: "ShareId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShareCharts_PnFCharts_ChartId",
                table: "ShareCharts",
                column: "ChartId",
                principalTable: "PnFCharts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShareCharts_PnFCharts_ChartId",
                table: "ShareCharts");

            migrationBuilder.DropIndex(
                name: "IX_ShareCharts_ChartId",
                table: "ShareCharts");

            migrationBuilder.DropIndex(
                name: "IX_ShareCharts_ShareId",
                table: "ShareCharts");

            migrationBuilder.CreateIndex(
                name: "IX_ShareCharts_ShareId",
                table: "ShareCharts",
                column: "ShareId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShareCharts_PnFCharts_ShareId",
                table: "ShareCharts",
                column: "ShareId",
                principalTable: "PnFCharts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
