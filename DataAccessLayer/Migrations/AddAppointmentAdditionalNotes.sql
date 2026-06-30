-- Add AdditionalNotes column to Appointments table (nullable).
-- Run against your Tabibak database if you get: Invalid column name 'AdditionalNotes'.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'AdditionalNotes')
BEGIN
    ALTER TABLE [Appointments] ADD [AdditionalNotes] nvarchar(2000) NULL;
END
GO
