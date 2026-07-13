-- ============================================================================
-- LogoBI Semantik Katman - Metadata Şema ve Tohum (Seed) Scripti
-- Bu script idempotent'tir; tekrar çalıştırıldığında hata vermez veya çift kayıt üretmez.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. SCHEMA CREATION
-- ----------------------------------------------------------------------------

-- Tablo: LogicalSource
IF OBJECT_ID(N'dbo.LogicalSource', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LogicalSource (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DisplayName     NVARCHAR(200)  NOT NULL,
        PhysicalPattern NVARCHAR(400)  NOT NULL,
        Scope           NVARCHAR(20)   NOT NULL CONSTRAINT CK_LogicalSource_Scope CHECK (Scope IN ('firm', 'period')),
        Grain           NVARCHAR(50)   NOT NULL,
        DefaultFilters  NVARCHAR(1000) NULL,
        IsHidden        BIT            NOT NULL CONSTRAINT DF_LogicalSource_IsHidden DEFAULT 0,
        Alias           NVARCHAR(10)   NOT NULL
    );
END
GO

-- Tablo: Field
IF OBJECT_ID(N'dbo.Field', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Field (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SourceId        INT           NOT NULL CONSTRAINT FK_Field_LogicalSource FOREIGN KEY REFERENCES dbo.LogicalSource(Id),
        PhysicalColumn  NVARCHAR(128) NOT NULL,
        DisplayName     NVARCHAR(200) NOT NULL,
        DataType        NVARCHAR(20)  NOT NULL CONSTRAINT CK_Field_DataType CHECK (DataType IN ('string', 'int', 'decimal', 'date', 'bool')),
        Format          NVARCHAR(50)  NULL,
        Role            NVARCHAR(20)  NOT NULL CONSTRAINT CK_Field_Role CHECK (Role IN ('dimension', 'measure')),
        DefaultAgg      NVARCHAR(10)  NULL     CONSTRAINT CK_Field_DefaultAgg CHECK (DefaultAgg IN ('sum', 'avg', 'count', 'min', 'max')),
        IsHidden        BIT           NOT NULL CONSTRAINT DF_Field_IsHidden DEFAULT 0
    );
END
GO

-- Tablo: Relationship
IF OBJECT_ID(N'dbo.Relationship', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Relationship (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FromSourceId  INT           NOT NULL CONSTRAINT FK_Relationship_FromSource FOREIGN KEY REFERENCES dbo.LogicalSource(Id),
        ToSourceId    INT           NOT NULL CONSTRAINT FK_Relationship_ToSource FOREIGN KEY REFERENCES dbo.LogicalSource(Id),
        FromColumn    NVARCHAR(128) NOT NULL,
        ToColumn      NVARCHAR(128) NOT NULL,
        Cardinality   NVARCHAR(20)  NOT NULL CONSTRAINT CK_Relationship_Cardinality CHECK (Cardinality IN ('one_to_one', 'one_to_many')),
        JoinType      NVARCHAR(10)  NOT NULL CONSTRAINT DF_Relationship_JoinType DEFAULT 'left' CONSTRAINT CK_Relationship_JoinType CHECK (JoinType IN ('left'))
    );
END
GO

-- ----------------------------------------------------------------------------
-- 2. SEED DATA (En küçük dilim: Faturalar + Cari)
-- ----------------------------------------------------------------------------

-- LogicalSource Tohumları
SET IDENTITY_INSERT dbo.LogicalSource ON;

IF NOT EXISTS (SELECT 1 FROM dbo.LogicalSource WHERE Id = 1)
BEGIN
    INSERT INTO dbo.LogicalSource (Id, DisplayName, PhysicalPattern, Scope, Grain, DefaultFilters, IsHidden, Alias)
    VALUES (1, N'Faturalar', N'LG_{FIRMA}_{DONEM}_INVOICE', N'period', N'invoice_header', N'{a}.CANCELLED = 0', 0, N'INV'); -- TODO: DB'de doğrula
END

IF NOT EXISTS (SELECT 1 FROM dbo.LogicalSource WHERE Id = 2)
BEGIN
    INSERT INTO dbo.LogicalSource (Id, DisplayName, PhysicalPattern, Scope, Grain, DefaultFilters, IsHidden, Alias)
    VALUES (2, N'Cari', N'LG_{FIRMA}_CLCARD', N'firm', N'card', NULL, 0, N'CLC'); -- TODO: DB'de doğrula
END

SET IDENTITY_INSERT dbo.LogicalSource OFF;
GO

-- Field Tohumları - Faturalar (SourceId = 1)
IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 1 AND PhysicalColumn = N'NUMBER')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (1, N'NUMBER', N'Fatura No', N'string', NULL, N'dimension', NULL, 0); -- TODO: DB'de doğrula
END

IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 1 AND PhysicalColumn = N'DATE_')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (1, N'DATE_', N'Tarih', N'date', N'dd.MM.yyyy', N'dimension', NULL, 0); -- TODO: DB'de doğrula
END

IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 1 AND PhysicalColumn = N'NETTOTAL')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (1, N'NETTOTAL', N'Net Tutar', N'decimal', N'#,##0.00', N'measure', N'sum', 0); -- TODO: DB'de doğrula
END

IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 1 AND PhysicalColumn = N'CLIENTREF')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (1, N'CLIENTREF', N'CLIENTREF', N'int', NULL, N'dimension', NULL, 1); -- TODO: DB'de doğrula
END

IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 1 AND PhysicalColumn = N'LOGICALREF')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (1, N'LOGICALREF', N'LOGICALREF', N'int', NULL, N'dimension', NULL, 1); -- TODO: DB'de doğrula
END

-- Field Tohumları - Cari (SourceId = 2)
IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 2 AND PhysicalColumn = N'CODE')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (2, N'CODE', N'Cari Kodu', N'string', NULL, N'dimension', NULL, 0); -- TODO: DB'de doğrula
END

IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 2 AND PhysicalColumn = N'DEFINITION_')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (2, N'DEFINITION_', N'Cari Ünvan', N'string', NULL, N'dimension', NULL, 0); -- TODO: DB'de doğrula
END

IF NOT EXISTS (SELECT 1 FROM dbo.Field WHERE SourceId = 2 AND PhysicalColumn = N'LOGICALREF')
BEGIN
    INSERT INTO dbo.Field (SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden)
    VALUES (2, N'LOGICALREF', N'LOGICALREF', N'int', NULL, N'dimension', NULL, 1); -- TODO: DB'de doğrula
END
GO

-- Relationship Tohumu - Faturalar (1) -> Cari (2)
IF NOT EXISTS (
    SELECT 1 FROM dbo.Relationship 
    WHERE FromSourceId = 1 AND ToSourceId = 2 AND FromColumn = N'CLIENTREF' AND ToColumn = N'LOGICALREF'
)
BEGIN
    INSERT INTO dbo.Relationship (FromSourceId, ToSourceId, FromColumn, ToColumn, Cardinality, JoinType)
    VALUES (1, 2, N'CLIENTREF', N'LOGICALREF', N'one_to_many', N'left'); -- TODO: DB'de doğrula
END
GO
