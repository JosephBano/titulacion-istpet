using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Titan.Application.Common.Behaviors;

namespace Titan.Application.DependencyInjection;

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

        return services;
    }
}
