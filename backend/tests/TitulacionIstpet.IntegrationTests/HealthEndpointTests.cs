using System.Net;
using FluentAssertions;
using Xunit;

namespace TitulacionIstpet.IntegrationTests;

/// <summary>
/// Arranca la app completa en memoria. Verifica que el grafo de DI resuelve,
/// que es donde se rompen los errores de cableado.
/// </summary>
public class HealthEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Health_responde_ok()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/health");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ruta_inexistente_responde_404()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/no-existe");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
