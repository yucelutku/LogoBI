namespace LogoBI.Shared.Query;

public record ReportDefinition
{
    public int AnchorSourceId { get; init; }
    public IReadOnlyList<int> FieldIds { get; init; } = Array.Empty<int>();
    public IReadOnlyList<Filter> Filters { get; init; } = Array.Empty<Filter>();
}
