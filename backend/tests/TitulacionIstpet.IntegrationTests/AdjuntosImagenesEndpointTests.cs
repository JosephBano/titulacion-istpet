using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Domain.Entities;
using Xunit;

namespace TitulacionIstpet.IntegrationTests;

public class RepositorioAdjuntosPrueba : IRepositorioAdjuntosImagenes
{
    public void Agregar(AdjuntosImagenes entidad)
    {
        entidad.IdAdjuntosImagenes = 123;
    }

    public void Actualizar(AdjuntosImagenes entidad) { }
    public void Eliminar(AdjuntosImagenes entidad) { }

    public Task<AdjuntosImagenes?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult<AdjuntosImagenes?>(new AdjuntosImagenes
        {
            IdAdjuntosImagenes = id,
            NombreArchivos = "test.pdf",
            Extension = "pdf",
            MimeTypes = "application/pdf",
            TamanioBytes = 100,
            Ruta = "/evidencias/123_test.pdf"
        });
    }

    public Task<IReadOnlyList<AdjuntosImagenes>> ListarAsync(int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<AdjuntosImagenes>>([]);
    }

    public Task<int> ContarAsync(CancellationToken ct = default)
    {
        return Task.FromResult(0);
    }
}

public class UnitOfWorkPrueba : IUnitOfWork
{
    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) => Task.FromResult(1);
}

public class AdjuntosImagenesEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private HttpClient CrearClienteConfigurado()
    {
        var app = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRepositorioAdjuntosImagenes>();
            services.AddSingleton<IRepositorioAdjuntosImagenes, RepositorioAdjuntosPrueba>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IUnitOfWork, UnitOfWorkPrueba>();
        }));

        return app.CreateClient();
    }

    [Fact]
    public async Task Subir_archivo_valido_retorna_200()
    {
        using var cliente = CrearClienteConfigurado();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "archivo", "test.pdf");

        var respuesta = await cliente.PostAsync("/api/adjuntos-imagenes/subir", content);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Subir_archivo_formato_no_permitido_retorna_400()
    {
        using var cliente = CrearClienteConfigurado();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-msdownload");
        content.Add(fileContent, "archivo", "script_malicioso.exe");

        var respuesta = await cliente.PostAsync("/api/adjuntos-imagenes/subir", content);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Subir_archivo_vacio_retorna_400()
    {
        using var cliente = CrearClienteConfigurado();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "archivo", "vacio.pdf");

        var respuesta = await cliente.PostAsync("/api/adjuntos-imagenes/subir", content);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
