using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Adding_PnFSignals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PnFSignals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Signals = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    PnFChartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PnFSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PnFSignals_PnFCharts_PnFChartId",
                        column: x => x.PnFChartId,
                        principalTable: "PnFCharts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PnFSignals_PnFChartId",
                table: "PnFSignals",
                column: "PnFChartId");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[PnFSignals_UPDATE] ON[dbo].[PnFSignals]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.PnFSignals
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PnFSignals");
        }
    }
}
