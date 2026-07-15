using LogoBI.Shared.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace LogoBI.Server.Controllers;

public record SourceSummaryResponse(int Id, string DisplayName, string Grain, string Scope);

public record FieldSummaryResponse(
    int Id,
    int SourceId,
    string DisplayName,
    string DataType,
    string? Format,
    string Role,
    string? DefaultAggregation
);

public record RelationshipSummaryResponse(
    int FromSourceId,
    int ToSourceId,
    string Cardinality,
    string JoinType
);

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly IMetadataRepository _metadataRepository;

    public MetadataController(IMetadataRepository metadataRepository)
    {
        _metadataRepository = metadataRepository;
    }

    [HttpGet("sources")]
    public async Task<ActionResult<IReadOnlyList<SourceSummaryResponse>>> GetSources(CancellationToken cancellationToken)
    {
        var sources = await _metadataRepository.GetSourcesAsync(cancellationToken);
        var result = sources
            .Where(s => !s.IsHidden)
            .Select(s => new SourceSummaryResponse(s.Id, s.DisplayName, s.Grain, s.Scope))
            .ToList();
        return Ok(result);
    }

    [HttpGet("fields")]
    public async Task<ActionResult<IReadOnlyList<FieldSummaryResponse>>> GetFields([FromQuery] int? sourceId, CancellationToken cancellationToken)
    {
        var fields = await _metadataRepository.GetFieldsAsync(cancellationToken);
        var result = fields
            .Where(f => !f.IsHidden && (!sourceId.HasValue || f.SourceId == sourceId.Value))
            .Select(f => new FieldSummaryResponse(f.Id, f.SourceId, f.DisplayName, f.DataType, f.Format, f.Role, f.DefaultAgg))
            .ToList();
        return Ok(result);
    }

    [HttpGet("relationships")]
    public async Task<ActionResult<IReadOnlyList<RelationshipSummaryResponse>>> GetRelationships(CancellationToken cancellationToken)
    {
        var relationships = await _metadataRepository.GetRelationshipsAsync(cancellationToken);
        var result = relationships
            .Select(r => new RelationshipSummaryResponse(r.FromSourceId, r.ToSourceId, r.Cardinality, r.JoinType))
            .ToList();
        return Ok(result);
    }
}
