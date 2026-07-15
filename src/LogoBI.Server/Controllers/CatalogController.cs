using LogoBI.Shared.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace LogoBI.Server.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly IFirmPeriodCatalog _catalog;

    public CatalogController(IFirmPeriodCatalog catalog)
    {
        _catalog = catalog;
    }

    [HttpGet("firms-periods")]
    public async Task<ActionResult<IReadOnlyList<CatalogFirmInfo>>> GetFirmsPeriods(CancellationToken cancellationToken)
    {
        var result = await _catalog.GetFirmsPeriodsAsync(cancellationToken);
        return Ok(result);
    }
}
