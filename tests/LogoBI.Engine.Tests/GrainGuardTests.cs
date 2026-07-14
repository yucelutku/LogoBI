using LogoBI.Engine.Guard;
using LogoBI.Shared.Metadata;
using Xunit;

namespace LogoBI.Engine.Tests;

public class GrainGuardTests
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
        new Field { Id = 101, SourceId = 1, PhysicalColumn = "FICHENO", DisplayName = "Fatura No", Role = "dimension" },
        new Field { Id = 102, SourceId = 2, PhysicalColumn = "CODE", DisplayName = "Cari Kodu", Role = "dimension" },
        new Field { Id = 103, SourceId = 3, PhysicalColumn = "AMOUNT", DisplayName = "Miktar", Role = "measure", DefaultAgg = "sum" },
        new Field { Id = 104, SourceId = 1, PhysicalColumn = "NETTOTAL", DisplayName = "Net Tutar", Role = "measure", DefaultAgg = "sum" },
        new Field { Id = 105, SourceId = 3, PhysicalColumn = "INVOICEREF", DisplayName = "INVOICEREF", Role = "dimension", IsHidden = true }
    };

    [Fact]
    public void Validate_OnlyDimensions_DoesNotThrow()
    {
        var selected = new[] { _fields[0], _fields[1], _fields[4] }; // FICHENO, CODE, INVOICEREF
        GrainGuard.Validate(selected, _sources);
    }

    [Fact]
    public void Validate_SingleMeasureAndDimensions_DoesNotThrow()
    {
        var selected = new[] { _fields[0], _fields[1], _fields[3] }; // FICHENO, CODE, NETTOTAL (header measure)
        GrainGuard.Validate(selected, _sources);
    }

    [Fact]
    public void Validate_TwoMeasuresDifferentGrains_ThrowsGrainMixException()
    {
        var selected = new[] { _fields[3], _fields[2] }; // NETTOTAL (header), AMOUNT (line)
        var ex = Assert.Throws<GrainMixException>(() => GrainGuard.Validate(selected, _sources));
        Assert.Contains("invoice_header", ex.Message);
        Assert.Contains("invoice_line", ex.Message);
    }

    [Fact]
    public void Validate_DifferentGrainDimensions_DoesNotThrow()
    {
        var selected = new[] { _fields[0], _fields[4] }; // FICHENO (header dim), INVOICEREF (line dim)
        GrainGuard.Validate(selected, _sources);
    }
}
