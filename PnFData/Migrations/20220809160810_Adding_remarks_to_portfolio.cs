using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    /// <inheritdoc />
    public partial class Adding_remarks_to_portfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "PortfolioShares",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PortfolioEventResults",
                columns: table => new
                {
                    Tidm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShareName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Portfolio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Holding = table.Column<double>(type: "float", nullable: false),
                    AdjustedClose = table.Column<double>(type: "float", nullable: false),
                    NewEvents = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioEventResults");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "PortfolioShares");
        }
    }
}
