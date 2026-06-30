using Microsoft.Data.SqlClient;

namespace DataAccessLayer.Migrations;

/// <summary>
/// Ensures DigitalPrescriptionItems has Spotlights column and removes Frequency/DurationHowLong
/// so the app works even when the EF migration was not applied.
/// </summary>
public static class DigitalPrescriptionItemSpotlightsRunner
{
    public static async Task RunAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var tableExists = await TableExistsAsync(connection, cancellationToken).ConfigureAwait(false);
            if (!tableExists)
                return;

            var needsSpotlights = await IsSpotlightsMissingAsync(connection, cancellationToken).ConfigureAwait(false);
            if (needsSpotlights)
            {
                Console.WriteLine("🔄 Adding Spotlights to DigitalPrescriptionItems...");
                await AddSpotlightsColumnAsync(connection, cancellationToken).ConfigureAwait(false);
                Console.WriteLine("✅ DigitalPrescriptionItems.Spotlights added.");
            }

            await DropFrequencyIfExistsAsync(connection, cancellationToken).ConfigureAwait(false);
            await DropDurationHowLongIfExistsAsync(connection, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  DigitalPrescriptionItems Spotlights migration failed: {ex.Message}");
        }
    }

    private static async Task<bool> TableExistsAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = "SELECT 1 FROM sys.tables WHERE name = 'DigitalPrescriptionItems';";
        await using var cmd = new SqlCommand(sql, connection);
        var value = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return value != null && value != DBNull.Value;
    }

    private static async Task<bool> IsSpotlightsMissingAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
SELECT 1 FROM sys.columns
WHERE object_id = OBJECT_ID('dbo.DigitalPrescriptionItems') AND name = 'Spotlights';";
        await using var cmd = new SqlCommand(sql, connection);
        var value = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return value == null || value == DBNull.Value;
    }

    private static async Task AddSpotlightsColumnAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DigitalPrescriptionItems') AND name = 'Spotlights')
    ALTER TABLE dbo.DigitalPrescriptionItems ADD Spotlights nvarchar(2000) NULL;";
        await using var cmd = new SqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task DropFrequencyIfExistsAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DigitalPrescriptionItems') AND name = 'Frequency')
    ALTER TABLE dbo.DigitalPrescriptionItems DROP COLUMN Frequency;";
        await using var cmd = new SqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task DropDurationHowLongIfExistsAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DigitalPrescriptionItems') AND name = 'DurationHowLong')
    ALTER TABLE dbo.DigitalPrescriptionItems DROP COLUMN DurationHowLong;";
        await using var cmd = new SqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}
