using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_above_ema_flags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClosedAboveEma10",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ClosedAboveEma30",
                table: "ShareIndicators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentAboveEma10",
                table: "IndexIndicators",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentAboveEma30",
                table: "IndexIndicators",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAboveEma10",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "ClosedAboveEma30",
                table: "ShareIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAboveEma10",
                table: "IndexIndicators");

            migrationBuilder.DropColumn(
                name: "PercentAboveEma30",
                table: "IndexIndicators");
        }
    }
}
