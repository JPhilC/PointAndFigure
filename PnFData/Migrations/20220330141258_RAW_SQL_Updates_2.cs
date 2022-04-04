using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class RAW_SQL_Updates_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[IndexCharts_UPDATE] ON[dbo].[IndexCharts]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.IndexCharts
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[IndexIndicators_UPDATE] ON[dbo].[IndexIndicators]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.IndexIndicators
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[IndexRSIValues_UPDATE] ON[dbo].[IndexRSIValues]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.IndexRSIValues
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");


            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[ShareCharts_UPDATE] ON[dbo].[ShareCharts]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.ShareCharts
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[ShareIndicators_UPDATE] ON[dbo].[ShareIndicators]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.ShareIndicators
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[ShareRSIValues_UPDATE] ON[dbo].[ShareRSIValues]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.ShareRSIValues
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
