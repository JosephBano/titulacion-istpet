using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TitulacionIstpet.Application.Features.Complexivo;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Interfaces.Security;

namespace TitulacionIstpet.IntegrationTests;

public class ComplexivoEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ComplexivoEndpointTests(ApiFactory factory) => _factory = factory;

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("TITULACION_DOCENTE", "Examen Complexivo:ver", HttpStatusCode.Forbidden)]
    [InlineData("TITULACION_ADMIN", null, HttpStatusCode.Forbidden)]
    [InlineData("TITULACION_ADMIN", "Examen Complexivo:ver", HttpStatusCode.OK)]
    public async Task Consulta_exige_rol_y_permiso(string? rol, string? permiso, HttpStatusCode esperado)
    {
        using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRepositorioComplexivo>();
            services.AddSingleton<IRepositorioComplexivo, RepositorioPrueba>();
        }));
        using var cliente = app.CreateClient();
        if (rol != null)
        {
            var generador = app.Services.GetRequiredService<IJwtTokenGenerator>();
            var (token, _) = generador.GenerateAccessToken(new Usuarios { IdUsuario = 1, Nombre = "Prueba" },
                [rol], permiso == null ? [] : [permiso]);
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var respuesta = await cliente.GetAsync("/api/v1/complexivo/cohortes-carreras");

        Assert.Equal(esperado, respuesta.StatusCode);
    }

    private sealed class RepositorioPrueba : IRepositorioComplexivo
    {
        public Task<IReadOnlyList<CohorteCarreraComplexivoDto>> ListarCohortesCarrerasAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<CohorteCarreraComplexivoDto>>([]);
        public Task<IReadOnlyList<MallaComplexivoDto>> ListarMallaAsync(int idCohorteCarrera, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<MallaComplexivoDto>>([]);
        public Task<MallaComplexivoDto?> ObtenerAsync(int id, CancellationToken ct) => Task.FromResult<MallaComplexivoDto?>(null);
        public Task<int> GuardarAsync(int? id, GuardarMallaComplexivoDto dto, CancellationToken ct) => Task.FromResult(1);
    }
}
