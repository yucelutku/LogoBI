using LogoBI.Engine.Compile;
using LogoBI.Engine.Execution;
using LogoBI.Engine.Tokens;
using LogoBI.Server.Data;
using LogoBI.Shared.Query;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Logo BI Console Smoke Test ===\n");

try
{
    // 1. IConfiguration'ı appsettings'ten yükle
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
        .Build();

    // 2. SqlServerMetadataRepository'i AppDb ile kur, metadata'yı oku
    Console.WriteLine("--> Metadata yükleniyor (AppDb)...");
    var metadataRepository = new SqlServerMetadataRepository(configuration);
    var sources = await metadataRepository.GetSourcesAsync();
    var fields = await metadataRepository.GetFieldsAsync();
    var relationships = await metadataRepository.GetRelationshipsAsync();
    Console.WriteLine($"    [Tamam] {sources.Count} kaynak, {fields.Count} alan, {relationships.Count} ilişki yüklendi.\n");

    // 3. İhtiyacımız olan alanları (Field) veritabanı ID'lerine dinamik olarak eşleyelim
    var fichenoField = fields.FirstOrDefault(f => f.SourceId == 1 && string.Equals(f.PhysicalColumn, "FICHENO", StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException("AppDb içerisinde SourceId=1, PhysicalColumn='FICHENO' alanı bulunamadı.");
    var codeField = fields.FirstOrDefault(f => f.SourceId == 2 && string.Equals(f.PhysicalColumn, "CODE", StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException("AppDb içerisinde SourceId=2, PhysicalColumn='CODE' alanı bulunamadı.");
    var nettotalField = fields.FirstOrDefault(f => f.SourceId == 1 && string.Equals(f.PhysicalColumn, "NETTOTAL", StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException("AppDb içerisinde SourceId=1, PhysicalColumn='NETTOTAL' alanı bulunamadı.");

    // En üstte/kolay değiştirilebilir şekilde 3 Senaryo Tanımı
    var scenarios = new (string Title, ReportDefinition Report)[]
    {
        (
            "Senaryo 1: Düz Liste (Fatura No + Cari Kodu)",
            new ReportDefinition
            {
                AnchorSourceId = 1, // Faturalar
                FieldIds = new[] { fichenoField.Id, codeField.Id }
            }
        ),
        (
            "Senaryo 2: Yalnız Measure (Toplam Net Tutar)",
            new ReportDefinition
            {
                AnchorSourceId = 1, // Faturalar
                FieldIds = new[] { nettotalField.Id }
            }
        ),
        (
            "Senaryo 3: Measure + Dimension (Fatura Başına Net Tutar)",
            new ReportDefinition
            {
                AnchorSourceId = 1, // Faturalar
                FieldIds = new[] { fichenoField.Id, nettotalField.Id }
            }
        )
    };

    // 4. TokenContext'i ActiveContext.Firm/Period'dan al
    int firm = configuration.GetValue<int>("ActiveContext:Firm");
    int period = configuration.GetValue<int>("ActiveContext:Period");
    var tokenContext = new TokenContext { Firm = firm, Period = period };
    Console.WriteLine($"--> TokenContext: FIRMA = {tokenContext.Firm:D3}, DONEM = {tokenContext.Period:D2}");

    int? topN = configuration.GetValue<int?>("Executor:TopN");

    string activeConnectionName = configuration.GetValue<string>("LogoConnections:Active")
        ?? throw new InvalidOperationException("LogoConnections:Active yapılandırma değeri bulunamadı.");

    string logoConnectionString = configuration.GetValue<string>($"LogoConnections:Connections:{activeConnectionName}")
        ?? throw new InvalidOperationException($"'{activeConnectionName}' için Logo bağlantı dizesi bulunamadı.");

    int timeoutSeconds = configuration.GetValue<int>("Executor:CommandTimeoutSeconds");
    if (timeoutSeconds <= 0) timeoutSeconds = 30;

    var executor = new QueryExecutor(logoConnectionString, timeoutSeconds);

    for (int sIdx = 0; sIdx < scenarios.Length; sIdx++)
    {
        var scenario = scenarios[sIdx];
        Console.WriteLine($"\n========================================================================");
        Console.WriteLine($"=== {scenario.Title} ===");
        Console.WriteLine($"========================================================================");

        var compiledQuery = SqlCompiler.Compile(scenario.Report, tokenContext, sources, fields, relationships, topN: topN);

        Console.WriteLine("\n--- ÜRETİLEN SQL ---");
        Console.WriteLine(compiledQuery.Sql);
        Console.WriteLine("\n--- PARAMETRELER ---");
        if (compiledQuery.Parameters.Count == 0)
        {
            Console.WriteLine("(Parametre yok)");
        }
        else
        {
            foreach (var kvp in compiledQuery.Parameters)
            {
                Console.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }
        Console.WriteLine();

        Console.WriteLine($"--> Logo DB ({activeConnectionName}) üzerinde sorgu çalıştırılıyor...");
        var result = await executor.ExecuteAsync(compiledQuery);

        Console.WriteLine("\n--- SONUÇLAR ---");
        if (result.Columns.Count == 0)
        {
            Console.WriteLine("Sorgu sonucu hiç kolon döndürmedi.");
        }
        else
        {
            var colWidths = new int[result.Columns.Count];
            for (int i = 0; i < result.Columns.Count; i++)
            {
                colWidths[i] = Math.Max(result.Columns[i].Name.Length, result.Columns[i].DataType.Length) + 3;
            }

            foreach (var row in result.Rows)
            {
                for (int i = 0; i < row.Count && i < colWidths.Length; i++)
                {
                    string valStr = FormatValue(row[i]);
                    colWidths[i] = Math.Max(colWidths[i], valStr.Length + 3);
                }
            }

            // Kolon adları ve veri tipleri
            for (int i = 0; i < result.Columns.Count; i++)
            {
                Console.Write($"{result.Columns[i].Name} ({result.Columns[i].DataType})".PadRight(colWidths[i] + 6));
            }
            Console.WriteLine();

            // Ayırıcı çizgi
            for (int i = 0; i < result.Columns.Count; i++)
            {
                Console.Write(new string('-', colWidths[i] + 4).PadRight(colWidths[i] + 6));
            }
            Console.WriteLine();

            // Satırlar
            foreach (var row in result.Rows)
            {
                for (int i = 0; i < row.Count && i < result.Columns.Count; i++)
                {
                    string valStr = FormatValue(row[i]);
                    Console.Write(valStr.PadRight(colWidths[i] + 6));
                }
                Console.WriteLine();
            }
            Console.WriteLine($"\n--> Toplam {result.Rows.Count} satır başarıyla okundu.");
        }
    }
}
catch (Exception ex)
{
    // 9. Exception yakalama ve net mesaj basma
    Console.WriteLine($"\n[HATA] Smoke test çalışırken özel durum oluştu:");
    Console.WriteLine($"       Tür: {ex.GetType().Name}");
    Console.WriteLine($"       Mesaj: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"       Alt Mesaj ({ex.InnerException.GetType().Name}): {ex.InnerException.Message}");
    }
}

static string FormatValue(object? val)
{
    if (val is null || val is DBNull) return "NULL";
    if (val is double d) return d.ToString("#,##0.00");
    if (val is float f) return f.ToString("#,##0.00");
    if (val is decimal m) return m.ToString("#,##0.00");
    return val.ToString() ?? "NULL";
}
