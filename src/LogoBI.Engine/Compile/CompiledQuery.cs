namespace LogoBI.Engine.Compile;

public record CompiledQuery
{
    public string Sql { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();
}
