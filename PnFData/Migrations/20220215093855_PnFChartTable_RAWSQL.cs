using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class PnFChartTable_RAWSQL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[PnFCharts_UPDATE] ON[dbo].[PnFCharts]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.PnFCharts
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[PnFColumns_UPDATE] ON[dbo].[PnFColumns]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.PnFColumns
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[PnFBoxes_UPDATE] ON[dbo].[PnFBoxes]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.PnFBoxes
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
