-- Convert Appointments.AppointmentID from int to uniqueidentifier (Guid).
-- Run this against your Tabibak database to fix:
--   InvalidCastException: Unable to cast object of type 'System.Int32' to type 'System.Guid'
--
-- Prerequisites: Run AddAppointmentAdditionalNotes.sql and AddDoctorRatingColumns.sql if you haven't.
-- After running this script, mark the migration as applied (see bottom) or run: dotnet ef database update

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1) Drop DoctorRatings table if it exists (it has FK to Appointments.AppointmentID)
IF OBJECT_ID('dbo.DoctorRatings', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[DoctorRatings];
END
GO

-- 2) Convert AppointmentID from int to uniqueidentifier only if it is currently int
DECLARE @colType sysname;
SELECT @colType = t.name
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Appointments') AND c.name = 'AppointmentID';

IF @colType = 'int'
BEGIN
    -- Add new Guid column
    ALTER TABLE [dbo].[Appointments] ADD [AppointmentIdNew] uniqueidentifier NULL;
    UPDATE [dbo].[Appointments] SET [AppointmentIdNew] = NEWID();
    ALTER TABLE [dbo].[Appointments] ALTER COLUMN [AppointmentIdNew] uniqueidentifier NOT NULL;

    -- Drop primary key and old column
    ALTER TABLE [dbo].[Appointments] DROP CONSTRAINT [PK_Appointments];
    ALTER TABLE [dbo].[Appointments] DROP COLUMN [AppointmentID];

    -- Rename new column to AppointmentID
    EXEC sp_rename 'dbo.Appointments.AppointmentIdNew', 'AppointmentID', 'COLUMN';
    ALTER TABLE [dbo].[Appointments] ADD CONSTRAINT [PK_Appointments] PRIMARY KEY ([AppointmentID]);
END

COMMIT TRANSACTION;
GO

-- 3) Ensure AdditionalNotes exists on Appointments
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Appointments') AND name = 'AdditionalNotes')
BEGIN
    ALTER TABLE [dbo].[Appointments] ADD [AdditionalNotes] nvarchar(2000) NULL;
END
GO

-- 4) Ensure Rating and RatingCount exist on Doctors
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Doctors') AND name = 'Rating')
BEGIN
    ALTER TABLE [dbo].[Doctors] ADD [Rating] decimal(3,2) NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Doctors') AND name = 'RatingCount')
BEGIN
    ALTER TABLE [dbo].[Doctors] ADD [RatingCount] int NOT NULL DEFAULT 0;
END
GO

-- 5) Recreate DoctorRatings table (matches EF migration)
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
END
GO

-- 6) Mark the EF migration as applied so "dotnet ef database update" does not try to run it again
IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260201120000_AppointmentGuidAdditionalNotesDoctorRating')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260201120000_AppointmentGuidAdditionalNotesDoctorRating', N'8.0.0');
END
GO
