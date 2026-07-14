-- ============================================================================
-- LogoBI Semantik Katman - Fiziksel Kolon Adları ve Şema Doğrulama Düzeltmeleri
-- Bu script idempotent'tir; tekrar çalıştırıldığında hata vermez.
-- ============================================================================

-- Faturalar (SourceId = 1) Fiziksel Kolon Düzeltmeleri
UPDATE dbo.Field
SET PhysicalColumn = N'FICHENO',
    DisplayName = N'Fatura No'
WHERE SourceId = 1 AND (PhysicalColumn IN (N'NUMBER', N'FICHENO') OR DisplayName = N'Fatura No');

UPDATE dbo.Field
SET PhysicalColumn = N'NETTOTAL',
    DisplayName = N'Net Tutar'
WHERE SourceId = 1 AND PhysicalColumn = N'NETTOTAL';

-- TODO: DATE_ int YYYYMMDD, tarih dilimine kadar working'i etkilemez
UPDATE dbo.Field
SET PhysicalColumn = N'DATE_',
    DisplayName = N'Tarih'
WHERE SourceId = 1 AND PhysicalColumn = N'DATE_';

UPDATE dbo.Field
SET PhysicalColumn = N'CLIENTREF'
WHERE SourceId = 1 AND PhysicalColumn = N'CLIENTREF';

UPDATE dbo.Field
SET PhysicalColumn = N'LOGICALREF'
WHERE SourceId = 1 AND PhysicalColumn = N'LOGICALREF';

-- Cari (SourceId = 2) Fiziksel Kolon Düzeltmeleri
UPDATE dbo.Field
SET PhysicalColumn = N'CODE',
    DisplayName = N'Cari Kodu'
WHERE SourceId = 2 AND PhysicalColumn = N'CODE';

UPDATE dbo.Field
SET PhysicalColumn = N'DEFINITION_',
    DisplayName = N'Cari Ünvan'
WHERE SourceId = 2 AND PhysicalColumn = N'DEFINITION_';

UPDATE dbo.Field
SET PhysicalColumn = N'LOGICALREF'
WHERE SourceId = 2 AND PhysicalColumn = N'LOGICALREF';

-- Stok Hareketleri (SourceId = 3) Fiziksel Kolon Düzeltmeleri
UPDATE dbo.Field
SET PhysicalColumn = N'AMOUNT',
    DisplayName = N'Miktar'
WHERE SourceId = 3 AND PhysicalColumn = N'AMOUNT';

UPDATE dbo.Field
SET PhysicalColumn = N'INVOICEREF',
    DisplayName = N'INVOICEREF'
WHERE SourceId = 3 AND PhysicalColumn = N'INVOICEREF';
GO
