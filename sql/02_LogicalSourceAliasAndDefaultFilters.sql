-- ============================================================================
-- LogoBI Semantik Katman - LogicalSource Alias ve DefaultFilters Güncellemesi
-- ============================================================================

-- LogicalSource'a Alias kolonu
IF COL_LENGTH('dbo.LogicalSource','Alias') IS NULL
    ALTER TABLE dbo.LogicalSource ADD Alias NVARCHAR(10) NULL;
GO

UPDATE dbo.LogicalSource SET Alias='INV' WHERE Id=1;
UPDATE dbo.LogicalSource SET Alias='CLC' WHERE Id=2;
UPDATE dbo.LogicalSource SET DefaultFilters='{a}.CANCELLED = 0' WHERE Id=1;
GO

-- doldurulduktan sonra NOT NULL yap:
ALTER TABLE dbo.LogicalSource ALTER COLUMN Alias NVARCHAR(10) NOT NULL;
GO
