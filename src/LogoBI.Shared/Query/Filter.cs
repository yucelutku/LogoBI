namespace LogoBI.Shared.Query;

public record Filter
{
    public int FieldId { get; init; }
    public string Operator { get; init; } = string.Empty;
    public object? Value { get; init; }
}
