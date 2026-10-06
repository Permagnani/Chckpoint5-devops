using System.Globalization;
using System.Text.Json.Serialization;
using DimDim.Web.Data;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC (front end) + endpoints REST (api/*)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Application Insights: lê a variável APPLICATIONINSIGHTS_CONNECTION_STRING
// configurada no App Service pelo script scripts/01-criar-recursos.sh
builder.Services.AddApplicationInsightsTelemetry();

// Registra o texto dos comandos SQL nas dependências (monitoramento do banco)
builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
    (module, _) => module.EnableSqlCommandTextInstrumentation = true);

// A connection string NÃO fica no código. No Azure ela vem da
// Connection String "DimDimDb" do App Service (SQLAZURECONNSTR_DimDimDb).
// Localmente use: dotnet user-secrets set "ConnectionStrings:DimDimDb" "..."
var connectionString = builder.Configuration.GetConnectionString("DimDimDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DimDimDb' não configurada. Defina-a no App Service ou via user-secrets.");
}

builder.Services.AddDbContext<DimDimContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Números chegam do formulário com ponto decimal (input type=number)
var culturaInvariante = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaInvariante),
    SupportedCultures = new[] { culturaInvariante },
    SupportedUICultures = new[] { culturaInvariante }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
