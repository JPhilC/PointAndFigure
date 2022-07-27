using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    /// <inheritdoc />
    public partial class Adding_additional_share_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BeneishMScore",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CROCI",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DebtToMarketCap",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DividendYearsGrowth",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DividendYearsPaid",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EBITMargin",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FixChargeCover",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ForecastYield",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ForecastYieldChange",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FreeCashConversion",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PensionToMarketCap",
                table: "Shares",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ROCE",
                table: "Shares",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeneishMScore",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "CROCI",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "DebtToMarketCap",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "DividendYearsGrowth",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "DividendYearsPaid",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "EBITMargin",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "FixChargeCover",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "ForecastYield",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "ForecastYieldChange",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "FreeCashConversion",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "PensionToMarketCap",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "ROCE",
                table: "Shares");
        }
    }
}
