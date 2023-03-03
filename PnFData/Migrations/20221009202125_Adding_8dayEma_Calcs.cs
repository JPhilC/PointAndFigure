using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    /// <inheritdoc />
    public partial class Adding_8dayEma_Calcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClosedAboveEma8d",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Ema8d",
                table: "ShareIndicators",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAboveEma8d",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "Ema8d",
                table: "ShareIndicators");
        }
    }
}
