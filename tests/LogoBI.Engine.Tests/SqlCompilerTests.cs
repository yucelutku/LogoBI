using LogoBI.Engine.Compile;
using LogoBI.Engine.Join;
using LogoBI.Engine.Tokens;
using LogoBI.Shared.Metadata;
using LogoBI.Shared.Query;
using Xunit;

namespace LogoBI.Engine.Tests;

public class SqlCompilerTests
{
    private readonly IReadOnlyList<LogicalSource> _sources = new[]
    {
        new LogicalSource
        {
            Id = 1,
            DisplayName = "Faturalar",
            PhysicalPattern = "LG_{FIRMA}_{DONEM}_INVOICE",
            Scope = "period",
            Grain = "invoice_header",
            DefaultFilters = "{a}.CANCELLED = 0",
            Alias = "INV"
        },
        new LogicalSource
        {
            Id = 2,
            DisplayName = "Cari",
            PhysicalPattern = "LG_{FIRMA}_CLCARD",
            Scope = "firm",
            Grain = "card",
            DefaultFilters = null,
            Alias = "CLC"
        },
        new LogicalSource
        {
            Id = 3,
            DisplayName = "Stok Hareketleri",
            PhysicalPattern = "LG_{FIRMA}_{DONEM}_STLINE",
            Scope = "period",
            Grain = "invoice_line",
            DefaultFilters = null,
            Alias = "STL"
        }
    };

    private readonly IReadOnlyList<Field> _fields = new[]
    {
        new Field
        {
            Id = 101,
            SourceId = 1,
            PhysicalColumn = "NUMBER",
            DisplayName = "Fatura No",
            Role = "dimension"
        },
        new Field
        {
            Id = 102,
            SourceId = 2,
            PhysicalColumn = "CODE",
            DisplayName = "Cari Kodu",
            Role = "dimension"
        },
        new Field
        {
            Id = 103,
            SourceId = 3,
            PhysicalColumn = "AMOUNT",
            DisplayName = "Miktar",
            Role = "measure"
        }
    };

    private readonly IReadOnlyList<Relationship> _relationships = new[]
    {
        new Relationship
        {
            Id = 1,
            FromSourceId = 1,
            ToSourceId = 2,
            FromColumn = "CLIENTREF",
            ToColumn = "LOGICALREF",
            Cardinality = "one_to_many",
            JoinType = "left"
        }
    };

    [Fact]
    public void Compile_HappyPath_GeneratesExpectedSqlAndEmptyParameters()
    {
        var report = new ReportDefinition
        {
            AnchorSourceId = 1,
            FieldIds = new[] { 101, 102 }
        };
        var ctx = new TokenContext { Firm = 9, Period = 1 };

        var result = SqlCompiler.Compile(report, ctx, _sources, _fields, _relationships);

        var expectedSql = @"SELECT INV.NUMBER, CLC.CODE
FROM LG_009_01_INVOICE INV
LEFT JOIN LG_009_CLCARD CLC ON CLC.LOGICALREF = INV.CLIENTREF
WHERE INV.CANCELLED = 0";

        Assert.Equal(NormalizeWhitespace(expectedSql), NormalizeWhitespace(result.Sql));
        Assert.Empty(result.Parameters);
    }

    [Fact]
    public void Compile_UnrelatedSourceField_ThrowsJoinNotPossibleException()
    {
        var report = new ReportDefinition
        {
            AnchorSourceId = 1,
            FieldIds = new[] { 101, 103 }
        };
        var ctx = new TokenContext { Firm = 9, Period = 1 };

        var ex = Assert.Throws<JoinNotPossibleException>(() =>
            SqlCompiler.Compile(report, ctx, _sources, _fields, _relationships));

        Assert.Contains("birleştirilemiyor", ex.Message);
    }

    [Fact]
    public async Task CompileAsync_WithFakeRepository_WorksForHappyPath()
    {
        var repo = new FakeMetadataRepository(_sources, _fields, _relationships);
        var report = new ReportDefinition
        {
            AnchorSourceId = 1,
            FieldIds = new[] { 101, 102 }
        };
        var ctx = new TokenContext { Firm = 9, Period = 1 };

        var result = await SqlCompiler.CompileAsync(report, ctx, repo);

        var expectedSql = @"SELECT INV.NUMBER, CLC.CODE
FROM LG_009_01_INVOICE INV
LEFT JOIN LG_009_CLCARD CLC ON CLC.LOGICALREF = INV.CLIENTREF
WHERE INV.CANCELLED = 0";

        Assert.Equal(NormalizeWhitespace(expectedSql), NormalizeWhitespace(result.Sql));
        Assert.Empty(result.Parameters);
    }

    private static string NormalizeWhitespace(string input)
    {
        return string.Join(" ", input.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
    }

    private class FakeMetadataRepository : IMetadataRepository
    {
        private readonly IReadOnlyList<LogicalSource> _sources;
        private readonly IReadOnlyList<Field> _fields;
        private readonly IReadOnlyList<Relationship> _relationships;

        public FakeMetadataRepository(
            IReadOnlyList<LogicalSource> sources,
            IReadOnlyList<Field> fields,
            IReadOnlyList<Relationship> relationships)
        {
            _sources = sources;
            _fields = fields;
            _relationships = relationships;
        }

        public Task<IReadOnlyList<LogicalSource>> GetSourcesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_sources);

        public Task<IReadOnlyList<Field>> GetFieldsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_fields);

        public Task<IReadOnlyList<Relationship>> GetRelationshipsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_relationships);
    }
}
