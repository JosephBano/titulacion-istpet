using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TitulacionIstpet.Application.Auth;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.ConfiguracionGeneral;
using TitulacionIstpet.Application.Features.Convocatorias;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Interfaces.Security;
using TitulacionIstpet.Infrastructure.Auth;
using TitulacionIstpet.Infrastructure.Persistence;
using TitulacionIstpet.Infrastructure.Persistence.Repositories;
using TitulacionIstpet.Infrastructure.Security;
using TitulacionIstpet.Infrastructure.Services;

namespace TitulacionIstpet.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        string? cadena = configuration.GetConnectionString("SigafiDb")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(cadena))
        {
            cadena = "Server=localhost;Database=sigafi_es;User=root;Password=;";
        }

        // La version se fija explicitamente para evitar autodetect en CI sin MySQL levantado
        var version = new MySqlServerVersion(new Version(5, 7, 44));

        services.AddDbContext<SigafiDbContext>(options =>
            options.UseMySql(cadena, version, mysql =>
            {
                mysql.EnableRetryOnFailure(maxRetryCount: 3, TimeSpan.FromSeconds(5), null);
                mysql.MigrationsHistoryTable("__ef_migrations_historial");
            }));

        // IUnitOfWork resuelve al mismo SigafiDbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SigafiDbContext>());

        // Repositorios existentes y nuevas features
        services.AddScoped<IRepositorioAdjuntosImagenes, RepositorioAdjuntosImagenes>();
        services.AddScoped<IRepositorioPostulaciones, RepositorioPostulaciones>();
        services.AddScoped<IRepositorioConfiguracionGeneral, RepositorioConfiguracionGeneral>();
        services.AddScoped<IRepositorioConvocatorias, RepositorioConvocatorias>();

        // Seguridad y Auth
        services.AddScoped<IVerificadorCredenciales, VerificadorCredencialesBcrypt>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Servicios de dominio / aplicación
        services.AddScoped<IRbacService, RbacService>();
        services.AddScoped<IRbacManagementService, RbacManagementService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICarrerasService, CarrerasService>();
        services.AddScoped<IModalidadesService, ModalidadesService>();
        services.AddScoped<IActoresService, ActoresService>();
        services.AddScoped<IAcademicoService, AcademicoService>();

        return services;
    }
}
