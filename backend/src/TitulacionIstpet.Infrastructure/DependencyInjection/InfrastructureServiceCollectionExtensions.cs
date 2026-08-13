using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Infrastructure.Persistence;
using TitulacionIstpet.Infrastructure.Persistence.Repositories;

namespace TitulacionIstpet.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var cadena = configuration.GetConnectionString("SigafiDb")
            ?? throw new InvalidOperationException(
                "Falta ConnectionStrings:SigafiDb. Copia appsettings.example.json a " +
                "appsettings.Development.json (git-ignored) o define la variable de entorno " +
                "ConnectionStrings__SigafiDb.");

        // La version se fija explicitamente: el autodetect abre una conexion en el arranque
        // y rompe el build de CI, donde no hay MySQL disponible.
        var version = new MySqlServerVersion(new Version(5, 7, 44));

        services.AddDbContext<SigafiDbContext>(options =>
            options.UseMySql(cadena, version, mysql =>
            {
                mysql.EnableRetryOnFailure(maxRetryCount: 3, TimeSpan.FromSeconds(5), null);
                mysql.MigrationsHistoryTable("__ef_migrations_historial");
            }));

        // IUnitOfWork resuelve al mismo SigafiDbContext (ambos scoped por peticion):
        // asi el repositorio y el caso de uso comparten ChangeTracker y transaccion.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SigafiDbContext>());

        services.AddScoped<IRepositorioAdjuntosImagenes, RepositorioAdjuntosImagenes>();

        return services;
    }
}
