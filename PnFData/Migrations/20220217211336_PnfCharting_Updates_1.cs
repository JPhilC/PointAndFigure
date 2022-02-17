using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class PnfCharting_Updates_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColumnType",
                table: "PnFColumns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ContainsNewYear",
                table: "PnFColumns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CurrentBoxIndex",
                table: "PnFColumns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "PnFBoxes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnType",
                table: "PnFColumns");

            migrationBuilder.DropColumn(
                name: "ContainsNewYear",
                table: "PnFColumns");

            migrationBuilder.DropColumn(
                name: "CurrentBoxIndex",
                table: "PnFColumns");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "PnFBoxes");
        }
    }
}
