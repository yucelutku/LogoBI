namespace LogoBI.Shared.Metadata;

public record Relationship
{
    public int Id { get; init; }
    public int FromSourceId { get; init; }
    public int ToSourceId { get; init; }
    public string FromColumn { get; init; } = string.Empty;
    public string ToColumn { get; init; } = string.Empty;
    public string Cardinality { get; init; } = string.Empty;
    public string JoinType { get; init; } = string.Empty;
}
