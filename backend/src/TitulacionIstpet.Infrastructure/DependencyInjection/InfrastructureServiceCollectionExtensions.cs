using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Domain.Repositories;
using TitulacionIstpet.Infrastructure.Persistence;
using TitulacionIstpet.Infrastructure.Persistence.Repositories;

namespace TitulacionIstpet.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var cadena = configuration.GetConnectionString("MySqlLegacy")
            ?? throw new InvalidOperationException(
                "Falta ConnectionStrings:MySqlLegacy. Copia appsettings.example.json a " +
                "appsettings.Development.json (git-ignored) o define la variable de entorno " +
                "ConnectionStrings__MySqlLegacy.");

        // La version se fija explicitamente: el autodetect abre una conexion en el arranque
        // y rompe el build de CI, donde no hay MySQL disponible.
        var version = new MySqlServerVersion(new Version(5, 7, 44));

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(cadena, version, mysql =>
            {
                mysql.EnableRetryOnFailure(maxRetryCount: 3, TimeSpan.FromSeconds(5), null);
                mysql.MigrationsHistoryTable("__ef_migrations_historial");
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IEstudianteRepository, EstudianteRepository>();

        return services;
    }
}
