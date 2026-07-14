using System.Text;
using LogoBI.Engine.Guard;
using LogoBI.Engine.Join;
using LogoBI.Engine.Tokens;
using LogoBI.Shared.Metadata;
using LogoBI.Shared.Query;

namespace LogoBI.Engine.Compile;

public static class SqlCompiler
{
    private const string TokenAlias = "{a}";
    private const string RoleMeasure = "measure";

    public static async Task<CompiledQuery> CompileAsync(
        ReportDefinition reportDefinition,
        TokenContext tokenContext,
        IMetadataRepository metadataRepository,
        CancellationToken cancellationToken = default,
        int? topN = null)
    {
        ArgumentNullException.ThrowIfNull(reportDefinition);
        ArgumentNullException.ThrowIfNull(tokenContext);
        ArgumentNullException.ThrowIfNull(metadataRepository);

        var sources = await metadataRepository.GetSourcesAsync(cancellationToken);
        var fields = await metadataRepository.GetFieldsAsync(cancellationToken);
        var relationships = await metadataRepository.GetRelationshipsAsync(cancellationToken);

        return Compile(reportDefinition, tokenContext, sources, fields, relationships, topN);
    }

    public static Task<CompiledQuery> CompileAsync(
        ReportDefinition reportDefinition,
        TokenContext tokenContext,
        IMetadataRepository metadataRepository,
        int? topN,
        CancellationToken cancellationToken = default)
    {
        return CompileAsync(reportDefinition, tokenContext, metadataRepository, cancellationToken, topN);
    }

    public static CompiledQuery Compile(
        ReportDefinition reportDefinition,
        TokenContext tokenContext,
        IReadOnlyList<LogicalSource> sources,
        IReadOnlyList<Field> fields,
        IReadOnlyList<Relationship> relationships,
        int? topN = null)
    {
        ArgumentNullException.ThrowIfNull(reportDefinition);
        ArgumentNullException.ThrowIfNull(tokenContext);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(relationships);

        var selectedFields = new List<Field>();
        foreach (var fieldId in reportDefinition.FieldIds)
        {
            var field = fields.FirstOrDefault(f => f.Id == fieldId)
                ?? throw new InvalidOperationException($"Field '{fieldId}' not found.");
            selectedFields.Add(field);
        }

        GrainGuard.Validate(selectedFields, sources);

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
        var groupByItems = new List<string>();
        bool hasMeasure = false;

        foreach (var field in selectedFields)
        {
            var source = sources.FirstOrDefault(s => s.Id == field.SourceId)
                ?? throw new InvalidOperationException($"Source '{field.SourceId}' for field '{field.DisplayName}' not found.");

            if (string.Equals(field.Role, RoleMeasure, StringComparison.OrdinalIgnoreCase))
            {
                hasMeasure = true;
                if (string.IsNullOrWhiteSpace(field.DefaultAgg))
                {
                    throw new InvalidOperationException($"Measure field '{field.DisplayName}' (Id={field.Id}) must have a default aggregation defined.");
                }
                selectItems.Add($"{field.DefaultAgg.ToUpperInvariant()}({source.Alias}.{field.PhysicalColumn})");
            }
            else
            {
                string colExpr = $"{source.Alias}.{field.PhysicalColumn}";
                selectItems.Add(colExpr);
                groupByItems.Add(colExpr);
            }
        }

        string resolvedAnchorTable = TokenResolver.ResolvePattern(anchorSource.PhysicalPattern, tokenContext);

        var sb = new StringBuilder();
        if (topN.HasValue)
        {
            if (topN.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(topN), "TopN cannot be negative.");
            }
            sb.Append("SELECT TOP ").Append(topN.Value).Append(' ').Append(string.Join(", ", selectItems));
        }
        else
        {
            sb.Append("SELECT ").Append(string.Join(", ", selectItems));
        }
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

        if (hasMeasure && groupByItems.Count > 0)
        {
            sb.AppendLine();
            sb.Append("GROUP BY ").Append(string.Join(", ", groupByItems));
        }

        return new CompiledQuery
        {
            Sql = sb.ToString(),
            Parameters = new Dictionary<string, object>()
        };
    }
}
