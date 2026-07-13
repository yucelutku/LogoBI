using System.Text;
using LogoBI.Engine.Join;
using LogoBI.Engine.Tokens;
using LogoBI.Shared.Metadata;
using LogoBI.Shared.Query;

namespace LogoBI.Engine.Compile;

public static class SqlCompiler
{
    private const string TokenAlias = "{a}";

    public static async Task<CompiledQuery> CompileAsync(
        ReportDefinition reportDefinition,
        TokenContext tokenContext,
        IMetadataRepository metadataRepository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reportDefinition);
        ArgumentNullException.ThrowIfNull(tokenContext);
        ArgumentNullException.ThrowIfNull(metadataRepository);

        var sources = await metadataRepository.GetSourcesAsync(cancellationToken);
        var fields = await metadataRepository.GetFieldsAsync(cancellationToken);
        var relationships = await metadataRepository.GetRelationshipsAsync(cancellationToken);

        return Compile(reportDefinition, tokenContext, sources, fields, relationships);
    }

    public static CompiledQuery Compile(
        ReportDefinition reportDefinition,
        TokenContext tokenContext,
        IReadOnlyList<LogicalSource> sources,
        IReadOnlyList<Field> fields,
        IReadOnlyList<Relationship> relationships)
    {
        ArgumentNullException.ThrowIfNull(reportDefinition);
        ArgumentNullException.ThrowIfNull(tokenContext);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(relationships);

        var anchorSource = sources.FirstOrDefault(s => s.Id == reportDefinition.AnchorSourceId)
            ?? throw new InvalidOperationException($"Anchor source '{reportDefinition.AnchorSourceId}' not found.");

        var resolvedJoins = AnchorJoinResolver.ResolveJoins(reportDefinition, sources, fields, relationships);

        var participatingSources = new List<LogicalSource> { anchorSource };
        foreach (var join in resolvedJoins)
        {
            participatingSources.Add(join.ToSource);
        }

        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in participatingSources)
        {
            if (string.IsNullOrWhiteSpace(source.Alias))
            {
                throw new InvalidOperationException($"Source '{source.DisplayName}' (Id={source.Id}) has no alias defined.");
            }
            if (!aliases.Add(source.Alias))
            {
                throw new InvalidOperationException($"Duplicate alias '{source.Alias}' detected among participating sources.");
            }
        }

        var selectItems = new List<string>();
        foreach (var fieldId in reportDefinition.FieldIds)
        {
            var field = fields.FirstOrDefault(f => f.Id == fieldId)
                ?? throw new InvalidOperationException($"Field '{fieldId}' not found.");

            var source = sources.FirstOrDefault(s => s.Id == field.SourceId)
                ?? throw new InvalidOperationException($"Source '{field.SourceId}' for field '{field.DisplayName}' not found.");

            selectItems.Add($"{source.Alias}.{field.PhysicalColumn}");
        }

        string resolvedAnchorTable = TokenResolver.ResolvePattern(anchorSource.PhysicalPattern, tokenContext);

        var sb = new StringBuilder();
        sb.Append("SELECT ").Append(string.Join(", ", selectItems));
        sb.AppendLine();
        sb.Append("FROM ").Append(resolvedAnchorTable).Append(' ').Append(anchorSource.Alias);

        foreach (var join in resolvedJoins)
        {
            string resolvedToTable = TokenResolver.ResolvePattern(join.ToSource.PhysicalPattern, tokenContext);
            sb.AppendLine();
            sb.Append("LEFT JOIN ").Append(resolvedToTable).Append(' ').Append(join.ToSource.Alias)
              .Append(" ON ").Append(join.ToSource.Alias).Append('.').Append(join.Relationship.ToColumn)
              .Append(" = ").Append(anchorSource.Alias).Append('.').Append(join.Relationship.FromColumn);
        }

        var whereClauses = new List<string>();
        foreach (var source in participatingSources)
        {
            if (!string.IsNullOrWhiteSpace(source.DefaultFilters))
            {
                string filter = source.DefaultFilters.Replace(TokenAlias, source.Alias, StringComparison.Ordinal);
                whereClauses.Add(filter);
            }
        }

        if (whereClauses.Count > 0)
        {
            sb.AppendLine();
            sb.Append("WHERE ").Append(string.Join(" AND ", whereClauses));
        }

        return new CompiledQuery
        {
            Sql = sb.ToString(),
            Parameters = new Dictionary<string, object>()
        };
    }
}
