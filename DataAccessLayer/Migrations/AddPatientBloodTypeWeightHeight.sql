-- Add BloodType, Weight, Height columns to Patients table only.
-- Run this script on your database if you get: Invalid column name 'BloodType', 'Height', 'Weight'
--
-- FROM D:\Tabibak (use your real server/database):
--   sqlcmd -S .\SQLEXPRESS04 -d Tabibak -E -i "DataAccessLayer\Migrations\AddPatientBloodTypeWeightHeight.sql"
--
-- Or open SQL Server Management Studio, connect to .\SQLEXPRESS04, database Tabibak,
-- open this file and execute (F5).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Patients') AND name = 'BloodType'
)
BEGIN
    ALTER TABLE dbo.Patients
    ADD BloodType nvarchar(20) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Patients') AND name = 'Weight'
)
BEGIN
    ALTER TABLE dbo.Patients
    ADD Weight decimal(7,2) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Patients') AND name = 'Height'
)
BEGIN
    ALTER TABLE dbo.Patients
    ADD Height decimal(5,2) NULL;
END
GO
