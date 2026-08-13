using Microsoft.AspNetCore.Mvc.Testing;

namespace Titan.IntegrationTests;

/// <summary>
/// Arranca la app con configuración propia para Integration Tests.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    static ApiFactory()
    {
        Poner("ASPNETCORE_ENVIRONMENT", "Development");
        Poner("ConnectionStrings__DefaultConnection",
            "Server=localhost;Port=3306;Database=titan_test;User Id=test;Password=test");
        Poner("Cors__OrigenesPermitidos__0", "http://localhost:4200");
    }

    /// <summary>No pisa lo que el entorno real ya definió.</summary>
    private static void Poner(string clave, string valor)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(clave)))
        {
            Environment.SetEnvironmentVariable(clave, valor);
        }
    }
}
