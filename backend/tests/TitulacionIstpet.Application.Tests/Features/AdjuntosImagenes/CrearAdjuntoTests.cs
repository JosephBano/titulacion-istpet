using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Tests.Features.AdjuntosImagenes;

public class CrearAdjuntoTests
{
    private readonly IRepositorioAdjuntosImagenes _repo = Substitute.For<IRepositorioAdjuntosImagenes>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly CrearAdjunto _sut;

    public CrearAdjuntoTests()
    {
        _sut = new CrearAdjunto(_repo, _uow);

        // El repo no conoce la PK hasta que EF Core la genera, asi que simulamos
        // ese momento con un retorno por argumento.
        _repo.When(r => r.Agregar(Arg.Any<AdjuntosImagene>()))
            .Do(ci => ci.Arg<AdjuntosImagene>().IdAdjuntosImagenes = 42);
    }

    [Fact]
    public async Task Crear_con_datos_validos_marca_la_entidad_y_persiste_una_sola_vez()
    {
        var comando = new CrearAdjuntoComando(
            "foto.png", "png", "image/png", 1024, "uploads/2026/foto.png");

        int id = await _sut.EjecutarAsync(comando);

        id.Should().Be(42);
        _repo.Received(1).Agregar(Arg.Is<AdjuntosImagene>(e =>
            e.NombreArchivos == "foto.png"
            && e.Extension == "png"
            && e.MimeTypes == "image/png"
            && e.TamanioBytes == 1024
            && e.Ruta == "uploads/2026/foto.png"));
        await _uow.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_sin_nombre_lanza_ValidacionException_y_no_persiste()
    {
        var comando = new CrearAdjuntoComando(null, "png", "image/png", 1024, "ruta");

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ValidacionException>()
            .Where(e => e.Errores.ContainsKey(nameof(CrearAdjuntoComando.NombreArchivos)));

        _repo.DidNotReceiveWithAnyArgs().Agregar(default!);
        await _uow.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }

    [Fact]
    public async Task Crear_con_tamano_negativo_lanza_ValidacionException()
    {
        var comando = new CrearAdjuntoComando("foto.png", "png", "image/png", -1, "ruta");

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ValidacionException>()
            .Where(e => e.Errores.ContainsKey(nameof(CrearAdjuntoComando.TamanioBytes)));
    }

    [Fact]
    public async Task Crear_con_nombre_mayor_a_90_caracteres_lanza_ValidacionException()
    {
        string nombreLargo = new('a', 91);
        var comando = new CrearAdjuntoComando(nombreLargo, "png", "image/png", 1024, "ruta");

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ValidacionException>()
            .Where(e => e.Errores.ContainsKey(nameof(CrearAdjuntoComando.NombreArchivos)));
    }

    [Fact]
    public async Task Crear_recorta_espacios_en_los_campos_de_texto()
    {
        var comando = new CrearAdjuntoComando(
            "  foto.png  ", "  png ", " image/png ", 1024, "  uploads/x  ");

        await _sut.EjecutarAsync(comando);

        _repo.Received(1).Agregar(Arg.Is<AdjuntosImagene>(e =>
            e.NombreArchivos == "foto.png"
            && e.Extension == "png"
            && e.MimeTypes == "image/png"
            && e.Ruta == "uploads/x"));
    }
}
