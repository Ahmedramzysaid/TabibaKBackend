using Microsoft.Data.SqlClient;

namespace DataAccessLayer.Migrations;

/// <summary>
/// Runs the AppointmentID int→Guid conversion on startup so the app can fix the database
/// using its own connection (no manual sqlcmd/SSMS needed).
/// </summary>
public static class AppointmentGuidMigrationRunner
{
    private const string MigrationId = "20260201120000_AppointmentGuidAdditionalNotesDoctorRating";

    public static async Task RunAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var needsConversion = await IsAppointmentIdIntAsync(connection, cancellationToken).ConfigureAwait(false);
            if (needsConversion)
            {
                Console.WriteLine("🔄 Converting Appointments.AppointmentID from int to Guid...");
                await DropDoctorRatingsIfExistsAsync(connection, cancellationToken).ConfigureAwait(false);
                await ConvertAppointmentIdToGuidAsync(connection, cancellationToken).ConfigureAwait(false);
                await EnsureAdditionalNotesAsync(connection, cancellationToken).ConfigureAwait(false);
                await EnsureDoctorRatingColumnsAsync(connection, cancellationToken).ConfigureAwait(false);
                await EnsureDoctorRatingsTableAsync(connection, cancellationToken).ConfigureAwait(false);
                await MarkMigrationAppliedAsync(connection, cancellationToken).ConfigureAwait(false);
                Console.WriteLine("✅ AppointmentID Guid conversion completed.");
            }

            // Always ensure default on AppointmentID so INSERT without value works (fixes NULL insert error)
            await EnsureAppointmentIdDefaultAsync(connection, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  AppointmentID Guid conversion failed: {ex.Message}");
            Console.WriteLine("⚠️  Run ConvertAppointmentIdToGuid.sql manually if you see InvalidCastException on save.");
        }
    }

    private static async Task<bool> IsAppointmentIdIntAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
SELECT t.name
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Appointments') AND c.name = 'AppointmentID';";
        await using var cmd = new SqlCommand(sql, connection);
        var value = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return string.Equals(value?.ToString(), "int", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task DropDoctorRatingsIfExistsAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF OBJECT_ID('dbo.DoctorRatings', 'U') IS NOT NULL
    DROP TABLE [dbo].[DoctorRatings];";
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 60 };
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task ConvertAppointmentIdToGuidAsync(SqlConnection connection, CancellationToken ct)
    {
        var steps = new[]
        {
            "ALTER TABLE [dbo].[Appointments] ADD [AppointmentIdNew] uniqueidentifier NULL",
            "UPDATE [dbo].[Appointments] SET [AppointmentIdNew] = NEWID()",
            "ALTER TABLE [dbo].[Appointments] ALTER COLUMN [AppointmentIdNew] uniqueidentifier NOT NULL",
            "ALTER TABLE [dbo].[Appointments] DROP CONSTRAINT [PK_Appointments]",
            "ALTER TABLE [dbo].[Appointments] DROP COLUMN [AppointmentID]",
            "EXEC sp_rename 'dbo.Appointments.AppointmentIdNew', 'AppointmentID', 'COLUMN'",
            "ALTER TABLE [dbo].[Appointments] ADD CONSTRAINT [PK_Appointments] PRIMARY KEY ([AppointmentID])"
        };
        foreach (var sql in steps)
        {
            await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 60 };
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
    }

    private static async Task EnsureAppointmentIdDefaultAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID('dbo.Appointments') AND c.name = 'AppointmentID')
BEGIN
    ALTER TABLE [dbo].[Appointments] ADD CONSTRAINT [DF_Appointments_AppointmentID] DEFAULT (NEWSEQUENTIALID()) FOR [AppointmentID];
END";
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task EnsureAdditionalNotesAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Appointments') AND name = 'AdditionalNotes')
    ALTER TABLE [dbo].[Appointments] ADD [AdditionalNotes] nvarchar(2000) NULL;";
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task EnsureDoctorRatingColumnsAsync(SqlConnection connection, CancellationToken ct)
    {
        var steps = new[]
        {
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Doctors') AND name = 'Rating')
    ALTER TABLE [dbo].[Doctors] ADD [Rating] decimal(3,2) NULL;",
            @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Doctors') AND name = 'RatingCount')
    ALTER TABLE [dbo].[Doctors] ADD [RatingCount] int NOT NULL DEFAULT 0;"
        };
        foreach (var sql in steps)
        {
            await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
    }

    private static async Task EnsureDoctorRatingsTableAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF OBJECT_ID('dbo.DoctorRatings', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DoctorRatings] (
        [Id] int NOT NULL IDENTITY(1,1),
        [AppointmentID] uniqueidentifier NOT NULL,
        [DoctorID] nvarchar(450) NOT NULL,
        [PatientID] nvarchar(450) NOT NULL,
        [Rating] tinyint NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_DoctorRatings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DoctorRatings_Appointments_AppointmentID] FOREIGN KEY ([AppointmentID]) REFERENCES [dbo].[Appointments] ([AppointmentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DoctorRatings_Doctors_DoctorID] FOREIGN KEY ([DoctorID]) REFERENCES [dbo].[Doctors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DoctorRatings_Patients_PatientID] FOREIGN KEY ([PatientID]) REFERENCES [dbo].[Patients] ([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE INDEX [IX_DoctorRatings_AppointmentID] ON [dbo].[DoctorRatings] ([AppointmentID]);
    CREATE INDEX [IX_DoctorRatings_DoctorID] ON [dbo].[DoctorRatings] ([DoctorID]);
    CREATE INDEX [IX_DoctorRatings_PatientID] ON [dbo].[DoctorRatings] ([PatientID]);
END";
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 60 };
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task MarkMigrationAppliedAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = @migrationId)
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (@migrationId, N'9.0.0');";
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@migrationId", MigrationId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}
