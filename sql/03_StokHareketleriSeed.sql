-- ============================================================================
-- LogoBI Semantik Katman - Stok Hareketleri Tohum (Seed) Scripti
-- Bu script idempotent'tir; tekrar çalıştırıldığında hata vermez veya çift kayıt üretmez.
-- ============================================================================

-- LogicalSource Tohumu (Id = 3: Stok Hareketleri)
SET IDENTITY_INSERT dbo.LogicalSource ON;

IF NOT EXISTS (SELECT 1 FROM dbo.LogicalSource WHERE Id = 3)
BEGIN
    INSERT INTO dbo.LogicalSource (Id, DisplayName, PhysicalPattern, Scope, Grain, DefaultFilters, IsHidden, Alias)
    VALUES (3, N'Stok Hareketleri', N'LG_{FIRMA}_{DONEM}_STLINE', N'period', N'invoice_line', N'{a}.CANCELLED = 0', 0, N'STL');
END
ELSE
BEGIN
    UPDATE dbo.LogicalSource
    SET DisplayName = N'Stok Hareketleri',
        PhysicalPattern = N'LG_{FIRMA}_{DONEM}_STLINE',
        Scope = N'period',
        Grain = N'invoice_line',
        DefaultFilters = N'{a}.CANCELLED = 0',
        IsHidden = 0,
        Alias = N'STL'
    WHERE Id = 3;
END

SET IDENTITY_INSERT dbo.LogicalSource OFF;
GO

-- Field Tohumları - Stok Hareketleri (SourceId = 3)
IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 3 AND PhysicalColumn = N'AMOUNT')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (3, N'AMOUNT', N'Miktar', N'decimal', N'#,##0.00', N'measure', N'sum', 0);
END
ELSE
BEGIN
    UPDATE dbo.Field
    SET DisplayName = N'Miktar',
        DataType = N'decimal',
        Format = N'#,##0.00',
        Role = N'measure',
        DefaultAgg = N'sum',
        IsHidden = 0
    WHERE SourceId = 3 AND PhysicalColumn = N'AMOUNT';
END

IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 3 AND PhysicalColumn = N'INVOICEREF')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (3, N'INVOICEREF', N'INVOICEREF', N'int', NULL, N'dimension', NULL, 1); -- join için, gizli
END
ELSE
BEGIN
    UPDATE dbo.Field
    SET DisplayName = N'INVOICEREF',
        DataType = N'int',
        Format = NULL,
        Role = N'dimension',
        DefaultAgg = NULL,
        IsHidden = 1
    WHERE SourceId = 3 AND PhysicalColumn = N'INVOICEREF';
END
GO

-- Relationship Tohumu - Stok Hareketleri (3) -> Faturalar (1)
IF NOT EXISTS (
    SELECT 1 FROM dbo.Relationship 
    WHERE FromSourceId = 3 AND ToSourceId = 1 AND FromColumn = N'INVOICEREF' AND ToColumn = N'LOGICALREF'
)
BEGIN
    INSERT INTO dbo.Relationship (FromSourceId, ToSourceId, FromColumn, ToColumn, Cardinality, JoinType)
    VALUES (3, 1, N'INVOICEREF', N'LOGICALREF', N'one_to_many', N'left');
END
ELSE
BEGIN
    UPDATE dbo.Relationship
    SET Cardinality = N'one_to_many',
        JoinType = N'left'
    WHERE FromSourceId = 3 AND ToSourceId = 1 AND FromColumn = N'INVOICEREF' AND ToColumn = N'LOGICALREF';
END
GO
