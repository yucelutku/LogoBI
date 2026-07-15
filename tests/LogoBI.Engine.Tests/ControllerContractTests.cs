using LogoBI.Engine.Execution;
using LogoBI.Server.Controllers;
using LogoBI.Server.Data;
using LogoBI.Shared.Metadata;
using LogoBI.Shared.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LogoBI.Engine.Tests;

public class ControllerContractTests
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
        new Field { Id = 101, SourceId = 1, PhysicalColumn = "FICHENO", DisplayName = "Fatura No", DataType = "varchar", Role = "dimension" },
        new Field { Id = 102, SourceId = 2, PhysicalColumn = "CODE", DisplayName = "Cari Kodu", DataType = "varchar", Role = "dimension" },
        new Field { Id = 103, SourceId = 3, PhysicalColumn = "AMOUNT", DisplayName = "Miktar", DataType = "float", Role = "measure", DefaultAgg = "sum" },
        new Field { Id = 104, SourceId = 1, PhysicalColumn = "NETTOTAL", DisplayName = "Net Tutar", DataType = "float", Role = "measure", DefaultAgg = "sum" },
        new Field { Id = 105, SourceId = 3, PhysicalColumn = "INVOICEREF", DisplayName = "INVOICEREF", DataType = "int", Role = "dimension", IsHidden = true }
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

    private IConfiguration CreateConfig()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ActiveContext:Firm"] = "126",
            ["ActiveContext:Period"] = "1",
            ["Executor:TopN"] = "100",
            ["Executor:CommandTimeoutSeconds"] = "30"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    [Fact]
    public async Task GetSources_ReturnsSourceSummaryWithoutPhysicalPattern()
    {
        var repo = new FakeMetadataRepository(_sources, _fields, _relationships);
        var controller = new MetadataController(repo);

        var actionResult = await controller.GetSources(default);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var sources = Assert.IsAssignableFrom<IReadOnlyList<SourceSummaryResponse>>(okResult.Value);

        Assert.Equal(3, sources.Count);
        var faturalar = sources.First(s => s.Id == 1);
        Assert.Equal("Faturalar", faturalar.DisplayName);
        Assert.Equal("invoice_header", faturalar.Grain);
        Assert.Equal("period", faturalar.Scope);

        // Verify that SourceSummaryResponse does not contain PhysicalPattern property via reflection
        Assert.Null(typeof(SourceSummaryResponse).GetProperty("PhysicalPattern"));
    }

    [Fact]
    public async Task GetFields_WithOptionalSourceId_ReturnsFilteredFieldsWithoutPhysicalColumn()
    {
        var repo = new FakeMetadataRepository(_sources, _fields, _relationships);
        var controller = new MetadataController(repo);

        var allActionResult = await controller.GetFields(null, default);
        var allResult = Assert.IsAssignableFrom<IReadOnlyList<FieldSummaryResponse>>(((OkObjectResult)allActionResult.Result!).Value);
        Assert.Equal(4, allResult.Count); // 105 is hidden

        var filteredActionResult = await controller.GetFields(1, default);
        var filteredResult = Assert.IsAssignableFrom<IReadOnlyList<FieldSummaryResponse>>(((OkObjectResult)filteredActionResult.Result!).Value);
        Assert.Equal(2, filteredResult.Count); // 101, 104
        Assert.All(filteredResult, f => Assert.Equal(1, f.SourceId));

        // Verify no PhysicalColumn on DTO
        Assert.Null(typeof(FieldSummaryResponse).GetProperty("PhysicalColumn"));
    }

    [Fact]
    public async Task GetRelationships_ReturnsRelationshipsWithoutJoinColumns()
    {
        var repo = new FakeMetadataRepository(_sources, _fields, _relationships);
        var controller = new MetadataController(repo);

        var actionResult = await controller.GetRelationships(default);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var rels = Assert.IsAssignableFrom<IReadOnlyList<RelationshipSummaryResponse>>(okResult.Value);

        Assert.Single(rels);
        Assert.Equal(1, rels[0].FromSourceId);
        Assert.Equal(2, rels[0].ToSourceId);
        Assert.Equal("one_to_many", rels[0].Cardinality);
        Assert.Equal("left", rels[0].JoinType);

        Assert.Null(typeof(RelationshipSummaryResponse).GetProperty("FromColumn"));
        Assert.Null(typeof(RelationshipSummaryResponse).GetProperty("ToColumn"));
    }

    [Fact]
    public async Task Preview_GrainConflict_Returns422WithGrainConflictCode()
    {
        var repo = new FakeMetadataRepository(_sources, _fields, _relationships);
        var executor = new QueryExecutor("Server=(localdb)\\MSSQLLocalDB;Database=Dummy;Trusted_Connection=True;");
        var controller = new ReportController(repo, executor, CreateConfig());

        var req = new ReportPreviewRequest(
            new ReportDefinition
            {
                AnchorSourceId = 1,
                FieldIds = new[] { 104, 103 } // NETTOTAL (header) + AMOUNT (line) -> Grain mix!
            },
            126,
            1
        );

        var actionResult = await controller.Preview(req, default);
        var objResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(422, objResult.StatusCode);

        var err = Assert.IsType<ApiErrorResponse>(objResult.Value);
        Assert.Equal("grain_conflict", err.Code);
        Assert.Contains("invoice_header", err.Message);
        Assert.Contains("invoice_line", err.Message);
    }

    [Fact]
    public async Task Preview_UnjoinableField_Returns422WithUnjoinableFieldCode()
    {
        var repo = new FakeMetadataRepository(_sources, _fields, _relationships);
        var executor = new QueryExecutor("Server=(localdb)\\MSSQLLocalDB;Database=Dummy;Trusted_Connection=True;");
        var controller = new ReportController(repo, executor, CreateConfig());

        var req = new ReportPreviewRequest(
            new ReportDefinition
            {
                AnchorSourceId = 1,
                FieldIds = new[] { 101, 103 } // FICHENO (SourceId=1) + AMOUNT (SourceId=3 without join from 1->3)
            },
            126,
            1
        );

        var actionResult = await controller.Preview(req, default);
        var objResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(422, objResult.StatusCode);

        var err = Assert.IsType<ApiErrorResponse>(objResult.Value);
        Assert.Equal("unjoinable_field", err.Code);
        Assert.Contains("birleştirilemiyor", err.Message);
    }

    [Fact]
    public async Task Preview_InvalidDefinition_Returns400()
    {
        var repo = new FakeMetadataRepository(_sources, _fields, _relationships);
        var executor = new QueryExecutor("Server=(localdb)\\MSSQLLocalDB;Database=Dummy;Trusted_Connection=True;");
        var controller = new ReportController(repo, executor, CreateConfig());

        var req = new ReportPreviewRequest(
            new ReportDefinition
            {
                AnchorSourceId = 0, // Invalid anchor
                FieldIds = new[] { 101 }
            },
            126,
            1
        );

        var actionResult = await controller.Preview(req, default);
        var badResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badResult.StatusCode);

        var err = Assert.IsType<ApiErrorResponse>(badResult.Value);
        Assert.Equal("invalid_definition", err.Code);
    }

    [Fact]
    public async Task LiveIntegration_GetSources_ReturnsRealSourcesWithoutPhysicalNames()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(configPath))
        {
            configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../src/LogoBI.Server/appsettings.json"));
        }
        var config = new ConfigurationBuilder().AddJsonFile(configPath, optional: false).Build();

        var metadataRepo = new SqlServerMetadataRepository(config);
        var controller = new MetadataController(metadataRepo);

        var actionResult = await controller.GetSources(default);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var sources = Assert.IsAssignableFrom<IReadOnlyList<SourceSummaryResponse>>(okResult.Value);

        Assert.Contains(sources, s => string.Equals(s.DisplayName, "Faturalar", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(sources, s => string.Equals(s.DisplayName, "Cari", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(sources, s => string.Equals(s.DisplayName, "Stok Hareketleri", StringComparison.OrdinalIgnoreCase));
        Assert.All(sources, s => Assert.Null(typeof(SourceSummaryResponse).GetProperty("PhysicalPattern")));
    }

    [Fact]
    public async Task LiveIntegration_Preview_FaturalarAndCari_Returns100RowsFromDeva()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(configPath))
        {
            configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../src/LogoBI.Server/appsettings.json"));
        }
        var config = new ConfigurationBuilder().AddJsonFile(configPath, optional: false).Build();

        var metadataRepo = new SqlServerMetadataRepository(config);
        string activeConnectionName = config.GetValue<string>("LogoConnections:Active")!;
        string logoConnectionString = config.GetValue<string>($"LogoConnections:Connections:{activeConnectionName}")!;
        var executor = new QueryExecutor(logoConnectionString, 30);

        var controller = new ReportController(metadataRepo, executor, config);

        var fields = await metadataRepo.GetFieldsAsync();
        var fichenoField = fields.First(f => f.SourceId == 1 && f.PhysicalColumn.Equals("FICHENO", StringComparison.OrdinalIgnoreCase));
        var codeField = fields.First(f => f.SourceId == 2 && f.PhysicalColumn.Equals("CODE", StringComparison.OrdinalIgnoreCase));

        var req = new ReportPreviewRequest(
            new ReportDefinition
            {
                AnchorSourceId = 1,
                FieldIds = new[] { fichenoField.Id, codeField.Id }
            },
            126,
            1
        );

        var actionResult = await controller.Preview(req, default);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var previewResult = Assert.IsType<PreviewResponse>(okResult.Value);

        Assert.Equal(2, previewResult.Columns.Count);
        Assert.Equal(fichenoField.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), previewResult.Columns[0].Key);
        Assert.Equal(fichenoField.DisplayName, previewResult.Columns[0].DisplayName);
        Assert.Equal(codeField.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), previewResult.Columns[1].Key);
        Assert.Equal(codeField.DisplayName, previewResult.Columns[1].DisplayName);
        Assert.Equal(100, previewResult.Rows.Count);
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
