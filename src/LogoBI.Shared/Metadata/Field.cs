namespace LogoBI.Shared.Metadata;

public record Field
{
    public int Id { get; init; }
    public int SourceId { get; init; }
    public string PhysicalColumn { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public string? Format { get; init; }
    public string Role { get; init; } = string.Empty;
    public string? DefaultAgg { get; init; }
    public bool IsHidden { get; init; }
}
