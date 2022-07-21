using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    /// <inheritdoc />
    public partial class RAW_SQL_Updates_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[Portfolios_UPDATE] ON [dbo].[Portfolios]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.Portfolios
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");
            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[PortfolioShares_UPDATE] ON [dbo].[PortfolioShares]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.PortfolioShares
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
