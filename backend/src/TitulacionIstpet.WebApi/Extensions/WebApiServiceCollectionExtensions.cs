using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.WebApi.Services;

namespace TitulacionIstpet.WebApi.Extensions;

public static class WebApiServiceCollectionExtensions
{
    public const string PoliticaCorsFrontend = "FrontendAngular";

    public static IServiceCollection AddWebApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioActual, UsuarioActual>();

        string[] origenes = configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>()
            ?? ["http://localhost:4200"];

        services.AddCors(options => options.AddPolicy(PoliticaCorsFrontend, policy =>
            policy.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

        return services;
    }
}
