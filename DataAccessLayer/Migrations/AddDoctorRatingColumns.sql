-- Add Rating and RatingCount columns to Doctors table
-- Run this script against your Tabibak database if you get:
-- Invalid column name 'Rating'. Invalid column name 'RatingCount'.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Doctors') AND name = 'Rating')
BEGIN
    ALTER TABLE [Doctors] ADD [Rating] decimal(3,2) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Doctors') AND name = 'RatingCount')
BEGIN
    ALTER TABLE [Doctors] ADD [RatingCount] int NOT NULL DEFAULT 0;
END
GO
