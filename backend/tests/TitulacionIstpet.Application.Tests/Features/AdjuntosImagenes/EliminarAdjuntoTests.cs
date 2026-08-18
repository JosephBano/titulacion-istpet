using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;
using TitulacionIstpet.Domain.Entities;
using AdjuntosImagenesEntity = TitulacionIstpet.Domain.Entities.AdjuntosImagenes;

namespace TitulacionIstpet.Application.Tests.Features.AdjuntosImagenes;

public class EliminarAdjuntoTests
{
    private readonly IRepositorioAdjuntosImagenes _repo = Substitute.For<IRepositorioAdjuntosImagenes>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly EliminarAdjunto _sut;

    public EliminarAdjuntoTests()
    {
        _sut = new EliminarAdjunto(_repo, _uow);
    }

    [Fact]
    public async Task Eliminar_con_id_existente_marca_y_persiste()
    {
        var entidad = new AdjuntosImagenesEntity { IdAdjuntosImagenes = 11 };
        _repo.ObtenerPorIdAsync(11, Arg.Any<CancellationToken>()).Returns(entidad);

        await _sut.EjecutarAsync(11);

        _repo.Received(1).Eliminar(entidad);
        await _uow.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_con_id_inexistente_lanza_NoEncontradoException_y_no_persiste()
    {
        _repo.ObtenerPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((AdjuntosImagenesEntity?)null);

        var accion = () => _sut.EjecutarAsync(99);

        await accion.Should().ThrowAsync<NoEncontradoException>();
        _repo.DidNotReceiveWithAnyArgs().Eliminar(default!);
        await _uow.DidNotReceiveWithAnyArgs().GuardarCambiosAsync(default);
    }
}
