

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();   // WASM'ý dev'de debug edebilmek için
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();       // (1) WASM çýktýsýný (framework dosyalarý) servis et
app.UseStaticFiles();                // (2) wwwroot statik dosyalarý (index.html, css, js)

app.UseAuthorization();

app.MapControllers();                // (3) API uçlarý — bunlar öncelikli eþleþir
app.MapFallbackToFile("index.html"); // (4) API'ye uymayan her yol WASM'a düþer (SPA routing)

app.Run();