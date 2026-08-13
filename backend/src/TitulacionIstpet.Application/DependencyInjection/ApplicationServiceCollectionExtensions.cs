using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TitulacionIstpet.Application.Common.Behaviors;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;

namespace TitulacionIstpet.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var ensamblado = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(ensamblado);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(ensamblado);

        // Casos de uso de la feature de ejemplo. Cada uno se registra como
        // clase concreta (no interfaz) porque no hay polimorfismo que justifique
        // abstraerlos; el coste de un mock por test es trivial.
        services.AddScoped<CrearAdjunto>();
        services.AddScoped<ActualizarAdjunto>();
        services.AddScoped<EliminarAdjunto>();
        services.AddScoped<ObtenerAdjuntoPorId>();
        services.AddScoped<ListarAdjuntos>();

        return services;
    }
}
