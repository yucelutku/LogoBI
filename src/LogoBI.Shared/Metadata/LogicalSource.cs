namespace LogoBI.Shared.Metadata;

public record LogicalSource
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string PhysicalPattern { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string Grain { get; init; } = string.Empty;
    public string? DefaultFilters { get; init; }
    public bool IsHidden { get; init; }
}
