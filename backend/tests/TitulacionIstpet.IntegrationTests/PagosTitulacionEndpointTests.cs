using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using TitulacionIstpet.Application.DTOs.Pagos;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Constants;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Interfaces.Security;
using Xunit;

namespace TitulacionIstpet.IntegrationTests;

public class PagosTitulacionEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public PagosTitulacionEndpointTests(ApiFactory factory) => _factory = factory;

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("TITULACION_DOCENTE", HttpStatusCode.Forbidden)]
    [InlineData(PagosConstantes.Roles.Secretaria, HttpStatusCode.OK)]
    [InlineData(PagosConstantes.Roles.Contabilidad, HttpStatusCode.OK)]
    [InlineData(PagosConstantes.Roles.TitulacionAdmin, HttpStatusCode.OK)]
    public async Task Consulta_configuraciones_valida_roles(string? rol, HttpStatusCode esperado)
    {
        var mockService = Substitute.For<IPagosTitulacionService>();
        mockService.ListarConfiguracionesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ConfiguracionPagoModalidadDto>());

        using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPagosTitulacionService>();
            services.AddSingleton(mockService);
        }));

        using var cliente = app.CreateClient();
        if (rol != null)
        {
            var generador = app.Services.GetRequiredService<IJwtTokenGenerator>();
            var (token, _) = generador.GenerateAccessToken(
                new Usuarios { IdUsuario = 1, Nombre = "Usuario Prueba" },
                [rol],
                []);
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var respuesta = await cliente.GetAsync("/api/v1/pagos-titulacion/configuraciones");
        Assert.Equal(esperado, respuesta.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData(PagosConstantes.Roles.Secretaria, HttpStatusCode.Forbidden)]
    [InlineData(PagosConstantes.Roles.Contabilidad, HttpStatusCode.Forbidden)]
    [InlineData(PagosConstantes.Roles.TitulacionAdmin, HttpStatusCode.OK)]
    public async Task Guardar_configuracion_solo_permite_titulacion_admin(string? rol, HttpStatusCode esperado)
    {
        var mockService = Substitute.For<IPagosTitulacionService>();
        mockService.GuardarConfiguracionAsync(Arg.Any<GuardarConfiguracionPagoModalidadDto>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ConfiguracionPagoModalidadDto(1, 1, "Complexivo", null, 700, 0, 700, null, null, true, DateTime.UtcNow, null));

        using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPagosTitulacionService>();
            services.AddSingleton(mockService);
        }));

        using var cliente = app.CreateClient();
        if (rol != null)
        {
            var generador = app.Services.GetRequiredService<IJwtTokenGenerator>();
            var (token, _) = generador.GenerateAccessToken(
                new Usuarios { IdUsuario = 1, Nombre = "Usuario Prueba" },
                [rol],
                []);
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var dto = new GuardarConfiguracionPagoModalidadDto(1, null, 700, 0, 700);
        using var respuesta = await cliente.PostAsJsonAsync("/api/v1/pagos-titulacion/configuraciones", dto);
        Assert.Equal(esperado, respuesta.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("TITULACION_DOCENTE", HttpStatusCode.Forbidden)]
    [InlineData(PagosConstantes.Roles.Contabilidad, HttpStatusCode.Forbidden)]
    [InlineData(PagosConstantes.Roles.Secretaria, HttpStatusCode.Created)]
    [InlineData(PagosConstantes.Roles.TitulacionAdmin, HttpStatusCode.Created)]
    public async Task Registrar_pago_solo_permite_secretaria_y_titulacion_admin(string? rol, HttpStatusCode esperado)
    {
        var mockService = Substitute.For<IPagosTitulacionService>();
        mockService.RegistrarPagoAsync(Arg.Any<RegistrarPagoTitulacionDto>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagoTitulacionDto(
                1, 10, "1720000001", "Estudiante Prueba", "Software", "Complexivo", 100, "BASE", "FAC-01", 500, "REGISTRADO",
                DateTime.UtcNow, 1, "Secretaria", null, null, null, null, null, null
            ));

        using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPagosTitulacionService>();
            services.AddSingleton(mockService);
        }));

        using var cliente = app.CreateClient();
        if (rol != null)
        {
            var generador = app.Services.GetRequiredService<IJwtTokenGenerator>();
            var (token, _) = generador.GenerateAccessToken(
                new Usuarios { IdUsuario = 1, Nombre = "Usuario Prueba" },
                [rol],
                []);
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var dto = new RegistrarPagoTitulacionDto(10, "BASE", "FAC-01", 500);
        using var respuesta = await cliente.PostAsJsonAsync("/api/v1/pagos-titulacion/registrar", dto);
        Assert.Equal(esperado, respuesta.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData(PagosConstantes.Roles.TitulacionAdmin, HttpStatusCode.Forbidden)]
    [InlineData(PagosConstantes.Roles.Secretaria, HttpStatusCode.Forbidden)]
    [InlineData(PagosConstantes.Roles.Contabilidad, HttpStatusCode.OK)]
    public async Task Revision_pago_solo_permite_rol_contabilidad(string? rol, HttpStatusCode esperado)
    {
        var mockService = Substitute.For<IPagosTitulacionService>();
        mockService.ProcesarRevisionContabilidadAsync(Arg.Any<int>(), Arg.Any<RevisionPagoDto>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagoTitulacionDto(
                1, 10, "1720000001", "Estudiante Prueba", "Software", "Complexivo", 100, "BASE", "FAC-01", 500, "APROBADO",
                DateTime.UtcNow, 1, "Secretaria", null, null, DateTime.UtcNow, 2, "Contabilidad", null
            ));

        using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPagosTitulacionService>();
            services.AddSingleton(mockService);
        }));

        using var cliente = app.CreateClient();
        if (rol != null)
        {
            var generador = app.Services.GetRequiredService<IJwtTokenGenerator>();
            var (token, _) = generador.GenerateAccessToken(
                new Usuarios { IdUsuario = 2, Nombre = "Contador" },
                [rol],
                []);
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var revision = new RevisionPagoDto(Aprobar: true);
        using var respuesta = await cliente.PutAsJsonAsync("/api/v1/pagos-titulacion/contabilidad/1/revision", revision);
        Assert.Equal(esperado, respuesta.StatusCode);
    }
}
