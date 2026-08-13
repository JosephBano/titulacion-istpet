using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Titan.Application.Interfaces;
using Titan.Domain.Interfaces.Security;
using Titan.Infrastructure.Security;
using Titan.Infrastructure.Services;

namespace Titan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRbacService, RbacService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRbacManagementService, RbacManagementService>();
        services.AddScoped<IAcademicoService, AcademicoService>();
        services.AddScoped<IModalidadesService, ModalidadesService>();
        services.AddScoped<IActoresService, ActoresService>();
        services.AddScoped<ICarrerasService, CarrerasService>();

        return services;
    }
}
