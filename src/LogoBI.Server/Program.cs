using LogoBI.Engine.Execution;
using LogoBI.Server.Data;
using LogoBI.Shared.Catalog;
using LogoBI.Shared.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMetadataRepository, SqlServerMetadataRepository>();
builder.Services.AddScoped<IFirmPeriodCatalog>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string activeConnectionName = config.GetValue<string>("LogoConnections:Active")
        ?? throw new InvalidOperationException("LogoConnections:Active yapılandırma değeri bulunamadı.");
    string logoConnectionString = config.GetValue<string>($"LogoConnections:Connections:{activeConnectionName}")
        ?? throw new InvalidOperationException($"'{activeConnectionName}' için Logo bağlantı dizesi bulunamadı.");
    return new SqlServerFirmPeriodCatalog(logoConnectionString);
});

builder.Services.AddScoped<QueryExecutor>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string activeConnectionName = config.GetValue<string>("LogoConnections:Active")
        ?? throw new InvalidOperationException("LogoConnections:Active yapılandırma değeri bulunamadı.");
    string logoConnectionString = config.GetValue<string>($"LogoConnections:Connections:{activeConnectionName}")
        ?? throw new InvalidOperationException($"'{activeConnectionName}' için Logo bağlantı dizesi bulunamadı.");
    int timeoutSeconds = config.GetValue<int>("Executor:CommandTimeoutSeconds");
    if (timeoutSeconds <= 0) timeoutSeconds = 30;
    return new QueryExecutor(logoConnectionString, timeoutSeconds);
});
builder.Services.AddScoped<Executor>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string activeConnectionName = config.GetValue<string>("LogoConnections:Active")
        ?? throw new InvalidOperationException("LogoConnections:Active yapılandırma değeri bulunamadı.");
    string logoConnectionString = config.GetValue<string>($"LogoConnections:Connections:{activeConnectionName}")
        ?? throw new InvalidOperationException($"'{activeConnectionName}' için Logo bağlantı dizesi bulunamadı.");
    int timeoutSeconds = config.GetValue<int>("Executor:CommandTimeoutSeconds");
    if (timeoutSeconds <= 0) timeoutSeconds = 30;
    return new Executor(logoConnectionString, timeoutSeconds);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
