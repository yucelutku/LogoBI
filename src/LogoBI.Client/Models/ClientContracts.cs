using LogoBI.Shared.Query;

namespace LogoBI.Client.Models;

public record ClientSourceInfo(int Id, string DisplayName, string Grain, string Scope);

public record ClientFieldInfo(
    int Id,
    int SourceId,
    string DisplayName,
    string DataType,
    string? Format,
    string Role,
    string? DefaultAggregation
);

public record ClientColumnInfo(string Key, string DisplayName, string? Format, string Role);

public record ClientQueryResult(IReadOnlyList<ClientColumnInfo> Columns, IReadOnlyList<IReadOnlyList<object?>> Rows);

public record ClientReportPreviewRequest(
    ReportDefinition? ReportDefinition,
    int? FirmaNo = null,
    int? DonemNo = null
);

public record ClientApiErrorResponse(string Code, string Message);

public class DropItem
{
    public ClientFieldInfo Field { get; set; } = default!;
    public string DropZone { get; set; } = "";
    public string SourceDisplayName { get; set; } = "";
}
