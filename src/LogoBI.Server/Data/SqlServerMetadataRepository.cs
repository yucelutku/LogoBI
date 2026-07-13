using Dapper;
using LogoBI.Shared.Metadata;
using Microsoft.Data.SqlClient;

namespace LogoBI.Server.Data;

public class SqlServerMetadataRepository : IMetadataRepository
{
    private const string TableLogicalSource = "LogicalSource";
    private const string TableField = "Field";
    private const string TableRelationship = "Relationship";

    private const string SelectSourcesSql =
        $"SELECT Id, DisplayName, PhysicalPattern, Scope, Grain, DefaultFilters, IsHidden FROM {TableLogicalSource}";

    private const string SelectFieldsSql =
        $"SELECT Id, SourceId, PhysicalColumn, DisplayName, DataType, Format, Role, DefaultAgg, IsHidden FROM {TableField}";

    private const string SelectRelationshipsSql =
        $"SELECT Id, FromSourceId, ToSourceId, FromColumn, ToColumn, Cardinality, JoinType FROM {TableRelationship}";

    private readonly string _connectionString;

    public SqlServerMetadataRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AppDb")
            ?? throw new InvalidOperationException("Connection string 'AppDb' was not found in configuration.");
    }

    public async Task<IReadOnlyList<LogicalSource>> GetSourcesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            SelectSourcesSql,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<LogicalSource>(command);
        return result.AsList();
    }

    public async Task<IReadOnlyList<Field>> GetFieldsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            SelectFieldsSql,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<Field>(command);
        return result.AsList();
    }

    public async Task<IReadOnlyList<Relationship>> GetRelationshipsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            SelectRelationshipsSql,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<Relationship>(command);
        return result.AsList();
    }
}
