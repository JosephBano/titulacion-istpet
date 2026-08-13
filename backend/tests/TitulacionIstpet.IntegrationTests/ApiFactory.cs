using Microsoft.AspNetCore.Mvc.Testing;

namespace TitulacionIstpet.IntegrationTests;

/// <summary>
/// Arranca la app con configuracion propia. Los appsettings reales estan git-ignored,
/// asi que los tests no pueden depender de ellos: en CI simplemente no existen.
///
/// La configuracion va por variables de entorno y no por ConfigureAppConfiguration porque
/// con minimal hosting el builder de Program.cs ya leyo la configuracion para cuando
/// WebApplicationFactory alcanza a intervenir.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    static ApiFactory()
    {
        Poner("ASPNETCORE_ENVIRONMENT", "Testing");
        Poner("ConnectionStrings__MySqlLegacy",
            "Server=localhost;Port=3306;Database=titulacion_test;User Id=test;Password=test");
        Poner("Cors__OrigenesPermitidos__0", "http://localhost:4200");
    }

    /// <summary>No pisa lo que el entorno real ya definio (p. ej. una MySQL de servicio en CI).</summary>
    private static void Poner(string clave, string valor)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(clave)))
        {
            Environment.SetEnvironmentVariable(clave, valor);
        }
    }
}
