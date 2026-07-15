namespace LogoBI.Shared.Catalog;

public record CatalogPeriodInfo(int PeriodNr, string Label);

public record CatalogFirmInfo(int FirmNr, string FirmName, IReadOnlyList<CatalogPeriodInfo> Periods);

public interface IFirmPeriodCatalog
{
    Task<IReadOnlyList<CatalogFirmInfo>> GetFirmsPeriodsAsync(CancellationToken cancellationToken = default);
}
