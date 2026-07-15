using LogoBI.Server.Controllers;
using LogoBI.Shared.Catalog;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LogoBI.Engine.Tests;

public class CatalogContractTests
{
    private class FakeFirmPeriodCatalog : IFirmPeriodCatalog
    {
        private readonly IReadOnlyList<CatalogFirmInfo> _firms;

        public FakeFirmPeriodCatalog(IReadOnlyList<CatalogFirmInfo> firms)
        {
            _firms = firms;
        }

        public Task<IReadOnlyList<CatalogFirmInfo>> GetFirmsPeriodsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_firms);
        }
    }

    [Fact]
    public async Task GetFirmsPeriods_ReturnsOk_WithCatalogList()
    {
        var fakeFirms = new List<CatalogFirmInfo>
        {
            new CatalogFirmInfo(126, "DEVA PLASTİK PVC GRANÜL SAN.TİC.LTD.ŞTİ.", new[]
            {
                new CatalogPeriodInfo(1, "01.01.2009 - 31.12.2009")
            }),
            new CatalogFirmInfo(200, "DÖNEMSİZ FİRMA", Array.Empty<CatalogPeriodInfo>())
        };

        var controller = new CatalogController(new FakeFirmPeriodCatalog(fakeFirms));

        var actionResult = await controller.GetFirmsPeriods(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedList = Assert.IsAssignableFrom<IReadOnlyList<CatalogFirmInfo>>(okResult.Value);

        Assert.Equal(2, returnedList.Count);
        Assert.Equal(126, returnedList[0].FirmNr);
        Assert.Equal("DEVA PLASTİK PVC GRANÜL SAN.TİC.LTD.ŞTİ.", returnedList[0].FirmName);
        Assert.Single(returnedList[0].Periods);
        Assert.Equal(1, returnedList[0].Periods[0].PeriodNr);
        Assert.Equal("01.01.2009 - 31.12.2009", returnedList[0].Periods[0].Label);

        Assert.Equal(200, returnedList[1].FirmNr);
        Assert.Empty(returnedList[1].Periods);
    }
}
