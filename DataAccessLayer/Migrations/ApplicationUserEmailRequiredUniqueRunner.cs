using Microsoft.Data.SqlClient;

namespace DataAccessLayer.Migrations;

/// <summary>
/// Ensures AspNetUsers.Email is NOT NULL and unique (unique index on NormalizedEmail).
/// Fixes existing NULL/empty emails with a placeholder so ALTER NOT NULL and unique index succeed.
/// </summary>
public static class ApplicationUserEmailRequiredUniqueRunner
{
    public static async Task RunAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            // 1. Set placeholder for NULL or empty Email/NormalizedEmail (unique per user: user-{Id}@required.tabibak)
            await SetPlaceholderForNullEmailsAsync(connection, cancellationToken).ConfigureAwait(false);

            // 2. Alter Email column to NOT NULL
            await MakeEmailNotNullAsync(connection, cancellationToken).ConfigureAwait(false);

            // 3. De-duplicate NormalizedEmail so unique index can be created (keep one per email, set duplicates to placeholder)
            await DeduplicateNormalizedEmailAsync(connection, cancellationToken).ConfigureAwait(false);

            // 4. Ensure unique index on NormalizedEmail (drop old non-unique if exists, create unique)
            await EnsureUniqueEmailIndexAsync(connection, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  ApplicationUser Email required/unique migration failed: {ex.Message}");
        }
    }

    private static async Task SetPlaceholderForNullEmailsAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
UPDATE AspNetUsers
SET
    Email = 'user-' + Id + '@required.tabibak',
    NormalizedEmail = UPPER('user-' + Id + '@required.tabibak')
WHERE Email IS NULL OR LTRIM(RTRIM(ISNULL(Email, ''))) = '';";
        await using var cmd = new SqlCommand(sql, connection);
        var updated = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (updated > 0)
            Console.WriteLine($"🔄 Updated {updated} user(s) with placeholder email (Email is now required).");
    }

    private static async Task MakeEmailNotNullAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'Email')
BEGIN
    DECLARE @sql nvarchar(500) = 'ALTER TABLE dbo.AspNetUsers ALTER COLUMN Email nvarchar(256) NOT NULL';
    EXEC sp_executesql @sql;
END";
        await using var cmd = new SqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task DeduplicateNormalizedEmailAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
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
WHERE d.rn > 1;";
        await using var cmd = new SqlCommand(sql, connection);
        var updated = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (updated > 0)
            Console.WriteLine($"🔄 De-duplicated email: updated {updated} user(s) with placeholder email (duplicate NormalizedEmail).");
    }

    private static async Task EnsureUniqueEmailIndexAsync(SqlConnection connection, CancellationToken ct)
    {
        // Drop default EmailIndex if it exists (may be non-unique)
        const string dropSql = @"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
    DROP INDEX EmailIndex ON dbo.AspNetUsers;";
        await using (var cmd = new SqlCommand(dropSql, connection))
        {
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        // Create unique index on NormalizedEmail (filter to allow index where not null; after our update all are non-null)
        const string createSql = @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_NormalizedEmail_Unique' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
    CREATE UNIQUE NONCLUSTERED INDEX IX_AspNetUsers_NormalizedEmail_Unique ON dbo.AspNetUsers (NormalizedEmail)
    WHERE NormalizedEmail IS NOT NULL;";
        await using (var cmd = new SqlCommand(createSql, connection))
        {
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
    }
}
