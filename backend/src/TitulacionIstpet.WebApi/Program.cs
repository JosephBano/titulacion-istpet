using Serilog;
using TitulacionIstpet.Application.DependencyInjection;
using TitulacionIstpet.Infrastructure.DependencyInjection;
using TitulacionIstpet.WebApi.Extensions;
using TitulacionIstpet.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ManejadorExcepcionesMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Titulación ISTPET API v1"));
}

app.UseCors(WebApiServiceCollectionExtensions.PoliticaCorsFrontend);

var staticPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(staticPath))
{
    Directory.CreateDirectory(staticPath);
}
var evidenciasPath = Path.Combine(staticPath, "evidencias");
if (!Directory.Exists(evidenciasPath))
{
    Directory.CreateDirectory(evidenciasPath);
}

app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health Checks estructurados (Liveness y Readiness)
var healthJsonOptions = new System.Text.Json.JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
};

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var isDev = app.Environment.IsDevelopment();

        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            environment = app.Environment.EnvironmentName,
            timestampUtc = DateTime.UtcNow,
            version = "1.0.0",
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = Math.Round(e.Value.Duration.TotalMilliseconds, 2),
                description = e.Value.Description,
                error = isDev ? e.Value.Exception?.Message : null
            })
        };

        await context.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(response, healthJsonOptions));
    }
}).WithTags("Infra");

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
}).WithTags("Infra");

app.Run();

/// <summary>Expuesto para que WebApplicationFactory pueda arrancar la app en los tests.</summary>
public partial class Program { }
