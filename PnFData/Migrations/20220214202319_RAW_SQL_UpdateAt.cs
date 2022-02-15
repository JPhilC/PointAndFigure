using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class RAW_SQL_UpdateAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[Shares_UPDATE] ON[dbo].[Shares]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.Shares
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[EodPrices_UPDATE] ON[dbo].[EodPrices]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.EodPrices
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[Indices_UPDATE] ON[dbo].[Indices]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.Indices
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[IndexDates_UPDATE] ON[dbo].[IndexDates]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.IndexDates
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[IndexShares_UPDATE] ON[dbo].[IndexShares]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.IndexShares
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
