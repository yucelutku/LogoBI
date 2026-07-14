namespace LogoBI.Engine.Execution;

public record QueryResult(IReadOnlyList<ColumnInfo> Columns, IReadOnlyList<IReadOnlyList<object?>> Rows);
