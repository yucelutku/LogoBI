using LogoBI.Engine.Compile;
using LogoBI.Engine.Execution;
using LogoBI.Engine.Guard;
using LogoBI.Engine.Join;
using LogoBI.Engine.Tokens;
using LogoBI.Shared.Metadata;
using LogoBI.Shared.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LogoBI.Server.Controllers;

public record ReportPreviewRequest(
    ReportDefinition? ReportDefinition,
    int? FirmaNo,
    int? DonemNo
);

public record ApiErrorResponse(string Code, string Message);

public record PreviewColumnInfo(string Key, string DisplayName, string? Format, string Role);

public record PreviewResponse(IReadOnlyList<PreviewColumnInfo> Columns, IReadOnlyList<IReadOnlyList<object?>> Rows);

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IMetadataRepository _metadataRepository;
    private readonly QueryExecutor _queryExecutor;
    private readonly IConfiguration _configuration;

    public ReportController(
        IMetadataRepository metadataRepository,
        QueryExecutor queryExecutor,
        IConfiguration configuration)
    {
        _metadataRepository = metadataRepository;
        _queryExecutor = queryExecutor;
        _configuration = configuration;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<PreviewResponse>> Preview([FromBody] ReportPreviewRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null || request.ReportDefinition == null)
            {
                return BadRequest(new ApiErrorResponse("invalid_definition", "Rapor tanımı (reportDefinition) eksik veya geçersiz."));
            }

            if (request.ReportDefinition.AnchorSourceId <= 0)
            {
                return BadRequest(new ApiErrorResponse("invalid_definition", "Rapor tanımı geçersiz: Çıpa tablo (anchorSourceId) belirtilmelidir."));
            }

            if (request.ReportDefinition.FieldIds == null || request.ReportDefinition.FieldIds.Count == 0)
            {
                return BadRequest(new ApiErrorResponse("invalid_definition", "Rapor tanımı geçersiz: En az bir alan (fieldIds) seçilmelidir."));
            }

            int firma = request.FirmaNo
                ?? _configuration.GetValue<int?>("ActiveContext:Firm")
                ?? 1;
            int donem = request.DonemNo
                ?? _configuration.GetValue<int?>("ActiveContext:Period")
                ?? 1;

            if (firma <= 0 || donem <= 0)
            {
                return BadRequest(new ApiErrorResponse("invalid_definition", "Geçersiz firma veya dönem numarası."));
            }

            var tokenContext = new TokenContext { Firm = firma, Period = donem };
            int? topN = _configuration.GetValue<int?>("Executor:TopN");

            var compiledQuery = await SqlCompiler.CompileAsync(
                request.ReportDefinition,
                tokenContext,
                _metadataRepository,
                cancellationToken,
                topN: topN
            );

            var result = await _queryExecutor.ExecuteAsync(compiledQuery, cancellationToken);

            var fields = await _metadataRepository.GetFieldsAsync(cancellationToken);
            if (result.Columns.Count != request.ReportDefinition.FieldIds.Count)
            {
                return StatusCode(500, new ApiErrorResponse("execution_error", "Sorgu sonucu kolon sayısı ile seçilen alan sayısı uyuşmuyor."));
            }

            var previewColumns = new List<PreviewColumnInfo>(request.ReportDefinition.FieldIds.Count);
            foreach (var fieldId in request.ReportDefinition.FieldIds)
            {
                var field = fields.FirstOrDefault(f => f.Id == fieldId);
                if (field == null)
                {
                    return StatusCode(500, new ApiErrorResponse("execution_error", $"Metadata içinde alan ID {fieldId} bulunamadı."));
                }

                previewColumns.Add(new PreviewColumnInfo(
                    Key: field.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    DisplayName: field.DisplayName,
                    Format: field.Format,
                    Role: field.Role
                ));
            }

            return Ok(new PreviewResponse(previewColumns, result.Rows));
        }
        catch (GrainMixException ex)
        {
            return StatusCode(422, new ApiErrorResponse("grain_conflict", ex.Message));
        }
        catch (JoinNotPossibleException ex)
        {
            return StatusCode(422, new ApiErrorResponse("unjoinable_field", ex.Message));
        }
        catch (Exception ex) when (ex is TimeoutException || ex is OperationCanceledException || (ex is SqlException sqlEx && (sqlEx.Number == -2 || sqlEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))))
        {
            return StatusCode(422, new ApiErrorResponse("query_timeout", "Sorgu çalışma süresi sınırını aştı veya zaman aşımına uğradı. Lütfen filtrelerinizi daraltarak tekrar deneyin."));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return BadRequest(new ApiErrorResponse("invalid_definition", $"Geçersiz rapor tanımı: {ex.Message}"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiErrorResponse("execution_error", "Sorgu çalıştırılırken sunucu tarafında bir hata oluştu."));
        }
    }
}
