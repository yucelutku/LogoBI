using LogoBI.Shared.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace LogoBI.Server.Controllers;

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
    public async Task<ActionResult<IReadOnlyList<LogicalSource>>> GetSources(CancellationToken cancellationToken)
    {
        var sources = await _metadataRepository.GetSourcesAsync(cancellationToken);
        return Ok(sources);
    }

    [HttpGet("fields")]
    public async Task<ActionResult<IReadOnlyList<Field>>> GetFields(CancellationToken cancellationToken)
    {
        var fields = await _metadataRepository.GetFieldsAsync(cancellationToken);
        return Ok(fields);
    }

    [HttpGet("relationships")]
    public async Task<ActionResult<IReadOnlyList<Relationship>>> GetRelationships(CancellationToken cancellationToken)
    {
        var relationships = await _metadataRepository.GetRelationshipsAsync(cancellationToken);
        return Ok(relationships);
    }
}
