using LogoBI.Engine.Compile;
using Microsoft.Data.SqlClient;

namespace LogoBI.Engine.Execution;

public class QueryExecutor
{
    private readonly string _connectionString;
    private readonly int _commandTimeoutSeconds;

    public QueryExecutor(string connectionString, int commandTimeoutSeconds = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        if (commandTimeoutSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(commandTimeoutSeconds), "CommandTimeoutSeconds must be positive.");
        }
        _connectionString = connectionString;
        _commandTimeoutSeconds = commandTimeoutSeconds;
    }

    public async Task<QueryResult> ExecuteAsync(CompiledQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(query.Sql);

        // TODO: Prod'da bu bağlantı 'sa' değil, db_datareader yetkili ayrı login olmalı (ADR-001, salt-okunurluğun DB-tarafı garantisi). Faz 0 dev'inde sa kabul.
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(query.Sql, connection);
        command.CommandTimeout = _commandTimeoutSeconds;

        if (query.Parameters != null)
        {
            foreach (var kvp in query.Parameters)
            {
                var param = command.CreateParameter();
                param.ParameterName = kvp.Key.StartsWith("@", StringComparison.Ordinal) ? kvp.Key : $"@{kvp.Key}";
                param.Value = kvp.Value ?? DBNull.Value;
                command.Parameters.Add(param);
            }
        }

        await using var reader = await command.ExecuteReaderAsync(ct);

        var columns = new List<ColumnInfo>();
        int fieldCount = reader.FieldCount;
        for (int i = 0; i < fieldCount; i++)
        {
            string colName = reader.GetName(i);
            string colType = reader.GetDataTypeName(i);
            columns.Add(new ColumnInfo(colName, colType));
        }

        var rows = new List<IReadOnlyList<object?>>();
        while (await reader.ReadAsync(ct))
        {
            var row = new object?[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                row[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }

        return new QueryResult(columns, rows);
    }
}

public class Executor : QueryExecutor
{
    public Executor(string connectionString, int commandTimeoutSeconds = 30)
        : base(connectionString, commandTimeoutSeconds)
    {
    }
}
