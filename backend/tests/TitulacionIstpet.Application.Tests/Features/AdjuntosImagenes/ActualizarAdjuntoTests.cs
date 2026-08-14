using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Tests.Features.AdjuntosImagenes;

public class ActualizarAdjuntoTests
{
    private readonly IRepositorioAdjuntosImagenes _repo = Substitute.For<IRepositorioAdjuntosImagenes>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ActualizarAdjunto _sut;

    public ActualizarAdjuntoTests()
    {
        _sut = new ActualizarAdjunto(_repo, _uow);
    }

    [Fact]
    public async Task Actualizar_con_id_existente_muta_la_entidad_y_persiste()
    {
        var existente = new AdjuntosImagene
        {
            IdAdjuntosImagenes = 7,
            NombreArchivos = "viejo.txt",
            Extension = "txt",
            MimeTypes = "text/plain",
            TamanioBytes = 100,
            Ruta = "uploads/viejo.txt"
        };
        _repo.ObtenerPorIdAsync(7, Arg.Any<CancellationToken>()).Returns(existente);

        var comando = new ActualizarAdjuntoComando(
            7, "nuevo.png", "png", "image/png", 2048, "uploads/nuevo.png");

        await _sut.EjecutarAsync(comando);

        _repo.Received(1).Actualizar(existente);
        existente.NombreArchivos.Should().Be("nuevo.png");
        existente.Extension.Should().Be("png");
        existente.MimeTypes.Should().Be("image/png");
        existente.TamanioBytes.Should().Be(2048);
        existente.Ruta.Should().Be("uploads/nuevo.png");
        await _uow.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_con_id_inexistente_lanza_NoEncontradoException_y_no_persiste()
    {
        _repo.ObtenerPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((AdjuntosImagene?)null);

        var comando = new ActualizarAdjuntoComando(
            99, "x.png", "png", "image/png", 1024, "ruta");

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<NoEncontradoException>();
        _repo.DidNotReceiveWithAnyArgs().Actualizar(default!);
        await _uow.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }

    [Fact]
    public async Task Actualizar_con_datos_invalidos_lanza_ValidacionException_y_no_persiste()
    {
        var comando = new ActualizarAdjuntoComando(
            7, " ", "png", "image/png", 1024, "ruta");

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ValidacionException>();
        await _repo.DidNotReceiveWithAnyArgs().ObtenerPorIdAsync(default, default);
        await _uow.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }
}
