using Dapper;
using LogoBI.Shared.Catalog;
using Microsoft.Data.SqlClient;

namespace LogoBI.Server.Data;

public class SqlServerFirmPeriodCatalog : IFirmPeriodCatalog
{
    private const string SelectFirmsPeriodsSql = @"
        SELECT CAST(CF.NR AS INT) AS FirmNr, CF.NAME AS FirmName,
               CAST(CP.NR AS INT) AS PeriodNr,
               CONVERT(NVARCHAR(MAX), CP.BEGDATE, 104) + ' - ' +
               CONVERT(NVARCHAR(MAX), CP.ENDDATE, 104) AS PeriodLabel
        FROM L_CAPIFIRM CF
        LEFT JOIN L_CAPIPERIOD CP ON CF.NR = CP.FIRMNR
        ORDER BY CF.NR, CP.NR";

    private readonly string _connectionString;

    public SqlServerFirmPeriodCatalog(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    private class CatalogRow
    {
        public int FirmNr { get; set; }
        public string? FirmName { get; set; }
        public int? PeriodNr { get; set; }
        public string? PeriodLabel { get; set; }
    }

    public async Task<IReadOnlyList<CatalogFirmInfo>> GetFirmsPeriodsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        var command = new CommandDefinition(
            SelectFirmsPeriodsSql,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<CatalogRow>(command);

        var dict = new Dictionary<int, (string FirmName, List<CatalogPeriodInfo> Periods)>();

        foreach (var row in rows)
        {
            if (!dict.TryGetValue(row.FirmNr, out var firmData))
            {
                firmData = (row.FirmName ?? "", new List<CatalogPeriodInfo>());
                dict[row.FirmNr] = firmData;
            }

            if (row.PeriodNr.HasValue && row.PeriodLabel != null)
            {
                firmData.Periods.Add(new CatalogPeriodInfo(row.PeriodNr.Value, row.PeriodLabel));
            }
        }

        var result = dict.Select(kvp => new CatalogFirmInfo(kvp.Key, kvp.Value.FirmName, kvp.Value.Periods)).ToList();
        return result;
    }
}
