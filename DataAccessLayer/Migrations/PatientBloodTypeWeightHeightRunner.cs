using Microsoft.Data.SqlClient;

namespace DataAccessLayer.Migrations;

/// <summary>
/// Ensures BloodType, Weight, Height columns exist on Patients so the app works
/// even when EF "No migrations were applied" (e.g. migration not in history).
/// </summary>
public static class PatientBloodTypeWeightHeightRunner
{
    public static async Task RunAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var needsColumns = await IsBloodTypeMissingAsync(connection, cancellationToken).ConfigureAwait(false);
            if (!needsColumns)
                return;

            Console.WriteLine("🔄 Adding BloodType, Weight, Height to Patients table...");
            await AddBloodTypeColumnAsync(connection, cancellationToken).ConfigureAwait(false);
            await AddWeightColumnAsync(connection, cancellationToken).ConfigureAwait(false);
            await AddHeightColumnAsync(connection, cancellationToken).ConfigureAwait(false);
            Console.WriteLine("✅ Patient BloodType/Weight/Height columns added.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Patient column migration failed: {ex.Message}");
        }
    }

    private static async Task<bool> IsBloodTypeMissingAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
SELECT 1 FROM sys.columns
WHERE object_id = OBJECT_ID('dbo.Patients') AND name = 'BloodType';";
        await using var cmd = new SqlCommand(sql, connection);
        var value = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return value == null || value == DBNull.Value;
    }

    private static async Task AddBloodTypeColumnAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Patients') AND name = 'BloodType')
    ALTER TABLE dbo.Patients ADD BloodType nvarchar(20) NULL;";
        await using var cmd = new SqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task AddWeightColumnAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Patients') AND name = 'Weight')
    ALTER TABLE dbo.Patients ADD Weight decimal(7,2) NULL;";
        await using var cmd = new SqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task AddHeightColumnAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Patients') AND name = 'Height')
    ALTER TABLE dbo.Patients ADD Height decimal(5,2) NULL;";
        await using var cmd = new SqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}
