using System.Net;
using FluentAssertions;
using Xunit;

namespace Titan.IntegrationTests;

/// <summary>
/// Arranca la app completa en memoria. Verifica que el grafo de DI resuelve,
/// que es donde se rompen los errores de cableado.
/// </summary>
public class HealthEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public HealthEndpointTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Swagger_responde_ok_en_testing()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/swagger/index.html");

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
