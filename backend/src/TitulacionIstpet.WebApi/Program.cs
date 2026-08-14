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

app.UseHttpsRedirection();
app.UseCors(WebApiServiceCollectionExtensions.PoliticaCorsFrontend);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { estado = "ok" })).WithTags("Infra");

app.Run();

/// <summary>Expuesto para que WebApplicationFactory pueda arrancar la app en los tests.</summary>
public partial class Program { }
