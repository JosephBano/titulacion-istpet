using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Tests.Features.AdjuntosImagenes;

public class ListarAdjuntosTests
{
    private readonly IRepositorioAdjuntosImagenes _repo = Substitute.For<IRepositorioAdjuntosImagenes>();
    private readonly ListarAdjuntos _sut;

    public ListarAdjuntosTests()
    {
        _sut = new ListarAdjuntos(_repo);
    }

    [Fact]
    public async Task Listar_con_pagina_y_tamano_validos_devuelve_items_y_total()
    {
        var entidades = new List<AdjuntosImagene>
        {
            new() { IdAdjuntosImagenes = 1, NombreArchivos = "a.png" },
            new() { IdAdjuntosImagenes = 2, NombreArchivos = "b.png" }
        };
        _repo.ListarAsync(2, 10, Arg.Any<CancellationToken>()).Returns(entidades);
        _repo.ContarAsync(Arg.Any<CancellationToken>()).Returns(42);

        var pagina = await _sut.EjecutarAsync(new ListarAdjuntosConsulta(Pagina: 2, TamanoPagina: 10));

        pagina.Pagina.Should().Be(2);
        pagina.TamanoPagina.Should().Be(10);
        pagina.Total.Should().Be(42);
        pagina.Items.Should().HaveCount(2);
        pagina.Items[0].IdAdjuntosImagenes.Should().Be(1);
        pagina.Items[1].IdAdjuntosImagenes.Should().Be(2);
    }

    [Fact]
    public async Task Listar_con_pagina_negativa_se_normaliza_a_uno()
    {
        _repo.ListarAsync(1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _repo.ContarAsync(Arg.Any<CancellationToken>()).Returns(0);

        var pagina = await _sut.EjecutarAsync(new ListarAdjuntosConsulta(Pagina: -5, TamanoPagina: 20));

        pagina.Pagina.Should().Be(1);
        await _repo.Received(1).ListarAsync(1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Listar_con_tamano_cero_usa_el_por_defecto()
    {
        _repo.ListarAsync(1, ListarAdjuntos.TamanoPaginaPorDefecto, Arg.Any<CancellationToken>())
            .Returns([]);
        _repo.ContarAsync(Arg.Any<CancellationToken>()).Returns(0);

        var pagina = await _sut.EjecutarAsync(new ListarAdjuntosConsulta(Pagina: 1, TamanoPagina: 0));

        pagina.TamanoPagina.Should().Be(ListarAdjuntos.TamanoPaginaPorDefecto);
    }

    [Fact]
    public async Task Listar_con_tamano_excesivo_se_acota_al_maximo()
    {
        _repo.ListarAsync(1, ListarAdjuntos.TamanoPaginaMaximo, Arg.Any<CancellationToken>())
            .Returns([]);
        _repo.ContarAsync(Arg.Any<CancellationToken>()).Returns(0);

        var pagina = await _sut.EjecutarAsync(new ListarAdjuntosConsulta(
            Pagina: 1, TamanoPagina: ListarAdjuntos.TamanoPaginaMaximo * 10));

        pagina.TamanoPagina.Should().Be(ListarAdjuntos.TamanoPaginaMaximo);
    }
}
