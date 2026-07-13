using LogoBI.Shared.Metadata;
using LogoBI.Shared.Query;

namespace LogoBI.Engine.Join;

public static class AnchorJoinResolver
{
    public static async Task<IReadOnlyList<ResolvedJoin>> ResolveJoinsAsync(
        ReportDefinition reportDefinition,
        IMetadataRepository metadataRepository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reportDefinition);
        ArgumentNullException.ThrowIfNull(metadataRepository);

        var sources = await metadataRepository.GetSourcesAsync(cancellationToken);
        var fields = await metadataRepository.GetFieldsAsync(cancellationToken);
        var relationships = await metadataRepository.GetRelationshipsAsync(cancellationToken);

        return ResolveJoins(reportDefinition, sources, fields, relationships);
    }

    public static async Task<IReadOnlyList<ResolvedJoin>> ResolveJoinsAsync(
        int anchorSourceId,
        IEnumerable<int> requiredSourceIds,
        IMetadataRepository metadataRepository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requiredSourceIds);
        ArgumentNullException.ThrowIfNull(metadataRepository);

        var sources = await metadataRepository.GetSourcesAsync(cancellationToken);
        var relationships = await metadataRepository.GetRelationshipsAsync(cancellationToken);

        return ResolveJoins(anchorSourceId, requiredSourceIds, sources, relationships);
    }

    public static IReadOnlyList<ResolvedJoin> ResolveJoins(
        ReportDefinition reportDefinition,
        IReadOnlyList<LogicalSource> sources,
        IReadOnlyList<Field> fields,
        IReadOnlyList<Relationship> relationships)
    {
        ArgumentNullException.ThrowIfNull(reportDefinition);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(relationships);

        var anchorSource = sources.FirstOrDefault(s => s.Id == reportDefinition.AnchorSourceId)
            ?? throw new InvalidOperationException($"Anchor source '{reportDefinition.AnchorSourceId}' not found.");

        var resolvedJoins = new List<ResolvedJoin>();
        var addedSourceIds = new HashSet<int> { anchorSource.Id };

        foreach (var fieldId in reportDefinition.FieldIds)
        {
            var field = fields.FirstOrDefault(f => f.Id == fieldId)
                ?? throw new InvalidOperationException($"Field '{fieldId}' not found.");

            if (addedSourceIds.Contains(field.SourceId))
            {
                continue;
            }

            var toSource = sources.FirstOrDefault(s => s.Id == field.SourceId)
                ?? throw new InvalidOperationException($"Source '{field.SourceId}' not found.");

            var rel = FindRelationship(anchorSource.Id, toSource.Id, relationships);
            if (rel == null)
            {
                throw new JoinNotPossibleException($"{field.DisplayName} alanı çıpa ile birleştirilemiyor.");
            }

            resolvedJoins.Add(new ResolvedJoin(toSource, rel));
            addedSourceIds.Add(toSource.Id);
        }

        return resolvedJoins;
    }

    public static IReadOnlyList<ResolvedJoin> ResolveJoins(
        int anchorSourceId,
        IEnumerable<int> requiredSourceIds,
        IReadOnlyList<LogicalSource> sources,
        IReadOnlyList<Relationship> relationships)
    {
        ArgumentNullException.ThrowIfNull(requiredSourceIds);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(relationships);

        var anchorSource = sources.FirstOrDefault(s => s.Id == anchorSourceId)
            ?? throw new InvalidOperationException($"Anchor source '{anchorSourceId}' not found.");

        var resolvedJoins = new List<ResolvedJoin>();
        var addedSourceIds = new HashSet<int> { anchorSource.Id };

        foreach (var toSourceId in requiredSourceIds)
        {
            if (addedSourceIds.Contains(toSourceId))
            {
                continue;
            }

            var toSource = sources.FirstOrDefault(s => s.Id == toSourceId)
                ?? throw new InvalidOperationException($"Source '{toSourceId}' not found.");

            var rel = FindRelationship(anchorSource.Id, toSource.Id, relationships);
            if (rel == null)
            {
                throw new JoinNotPossibleException($"{toSource.DisplayName} alanı çıpa ile birleştirilemiyor.");
            }

            resolvedJoins.Add(new ResolvedJoin(toSource, rel));
            addedSourceIds.Add(toSource.Id);
        }

        return resolvedJoins;
    }

    private static Relationship? FindRelationship(
        int anchorSourceId,
        int toSourceId,
        IReadOnlyList<Relationship> relationships)
    {
        foreach (var rel in relationships)
        {
            if (rel.FromSourceId == anchorSourceId && rel.ToSourceId == toSourceId)
            {
                return rel;
            }

            if (rel.FromSourceId == toSourceId && rel.ToSourceId == anchorSourceId)
            {
                return new Relationship
                {
                    Id = rel.Id,
                    FromSourceId = anchorSourceId,
                    ToSourceId = toSourceId,
                    FromColumn = rel.ToColumn,
                    ToColumn = rel.FromColumn,
                    Cardinality = rel.Cardinality,
                    JoinType = rel.JoinType
                };
            }
        }

        return null;
    }
}
