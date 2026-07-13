namespace LogoBI.Shared.Metadata;

public interface IMetadataRepository
{
    Task<IReadOnlyList<LogicalSource>> GetSourcesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Field>> GetFieldsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Relationship>> GetRelationshipsAsync(CancellationToken cancellationToken = default);
}
