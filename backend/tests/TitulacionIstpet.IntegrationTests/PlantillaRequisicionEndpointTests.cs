using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TitulacionIstpet.Application.Features.PlantillasRequisicion;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using TitulacionIstpet.Domain.Interfaces.Security;
using Xunit;

namespace TitulacionIstpet.IntegrationTests;

public class PlantillaRequisicionEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("TITULACION_DOCENTE", "Examen Complexivo:ver", HttpStatusCode.Forbidden)]
    [InlineData("TITULACION_ADMIN", null, HttpStatusCode.Forbidden)]
    [InlineData("TITULACION_ADMIN", "Examen Complexivo:ver", HttpStatusCode.OK)]
    public async Task Consulta_plantilla_exige_rol_y_permiso(string? rol, string? permiso, HttpStatusCode esperado)
    {
        using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRepositorioPlantillaRequisicion>();
            services.AddSingleton<IRepositorioPlantillaRequisicion, RepositorioPlantillaPrueba>();
        }));
        using var cliente = app.CreateClient();
        if (rol != null)
        {
            var generador = app.Services.GetRequiredService<IJwtTokenGenerator>();
            var (token, _) = generador.GenerateAccessToken(
                new Usuarios { IdUsuario = 1, Nombre = "Prueba" },
                [rol],
                permiso == null ? [] : [permiso]);
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var respuesta = await cliente.GetAsync("/api/v1/complexivo/plantilla-requisicion");

        Assert.Equal(esperado, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizacion_con_conflicto_retorna_409()
    {
        using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRepositorioPlantillaRequisicion>();
            services.AddSingleton<IRepositorioPlantillaRequisicion, RepositorioPlantillaConflictoPrueba>();
        }));
        using var cliente = app.CreateClient();
        var generador = app.Services.GetRequiredService<IJwtTokenGenerator>();
        var (token, _) = generador.GenerateAccessToken(
            new Usuarios { IdUsuario = 53, Nombre = "Pamela" },
            ["TITULACION_ADMIN"],
            ["Examen Complexivo:editar"]);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var comando = new ActualizarPlantillaRequisicionComandoDto(
            RevisionEsperada: 1,
            Definicion: PlantillaRequisicionBase.CrearDefinicionPredeterminada()
        );

        using var respuesta = await cliente.PutAsJsonAsync("/api/v1/complexivo/plantilla-requisicion", comando);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    private sealed class RepositorioPlantillaPrueba : IRepositorioPlantillaRequisicion
    {
        public Task<TitulPlantillaDocumento?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
            => Task.FromResult<TitulPlantillaDocumento?>(null);

        public Task<TitulPlantillaDocumento> GuardarOActualizarAsync(
            string codigo,
            string definicionJson,
            int revisionEsperada,
            int? usuarioId,
            CancellationToken ct = default)
        {
            return Task.FromResult(new TitulPlantillaDocumento
            {
                IdPlantillaDocumento = 1,
                Codigo = codigo,
                DefinicionJson = definicionJson,
                Revision = revisionEsperada + 1,
                ActualizadoPor = usuarioId,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            });
        }
    }

    private sealed class RepositorioPlantillaConflictoPrueba : IRepositorioPlantillaRequisicion
    {
        public Task<TitulPlantillaDocumento?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
            => Task.FromResult<TitulPlantillaDocumento?>(null);

        public Task<TitulPlantillaDocumento> GuardarOActualizarAsync(
            string codigo,
            string definicionJson,
            int revisionEsperada,
            int? usuarioId,
            CancellationToken ct = default)
        {
            throw new DominioException("Conflicto de concurrencia: la plantilla fue modificada por otro usuario.");
        }
    }
}
