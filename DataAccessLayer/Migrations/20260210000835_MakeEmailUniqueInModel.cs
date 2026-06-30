using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailUniqueInModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. De-duplicate NormalizedEmail: keep one row per email (lowest Id), set duplicates to unique placeholder
            migrationBuilder.Sql(@"
;WITH Duplicates AS (
    SELECT Id, NormalizedEmail,
           ROW_NUMBER() OVER (PARTITION BY NormalizedEmail ORDER BY Id) AS rn
    FROM AspNetUsers
    WHERE NormalizedEmail IS NOT NULL AND LTRIM(RTRIM(NormalizedEmail)) <> ''
)
UPDATE u
SET
    u.Email = 'user-' + u.Id + '@required.tabibak',
    u.NormalizedEmail = UPPER('user-' + u.Id + '@required.tabibak')
FROM AspNetUsers u
INNER JOIN Duplicates d ON d.Id = u.Id
WHERE d.rn > 1;");

            // 2. Create unique index (idempotent)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_NormalizedEmail_Unique' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
        DROP INDEX [EmailIndex] ON [AspNetUsers];
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AspNetUsers_NormalizedEmail_Unique] ON [AspNetUsers] ([NormalizedEmail])
    WHERE [NormalizedEmail] IS NOT NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_NormalizedEmail_Unique' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
    DROP INDEX [IX_AspNetUsers_NormalizedEmail_Unique] ON [AspNetUsers];");
        }
    }
}
