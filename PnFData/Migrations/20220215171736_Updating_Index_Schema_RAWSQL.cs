using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PnFData.Migrations
{
    public partial class Updating_Index_Schema_RAWSQL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE TRIGGER[dbo].[IndexValues_UPDATE] ON[dbo].[IndexValues]
                    AFTER UPDATE
                    AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF((SELECT TRIGGER_NESTLEVEL()) > 1) RETURN;

                            DECLARE @Id UniqueIdentifier

                            SELECT @Id = INSERTED.Id
                            FROM INSERTED

                            UPDATE dbo.IndexValues
                            SET UpdatedAt = GETDATE()
                            WHERE Id = @Id
                        END");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
