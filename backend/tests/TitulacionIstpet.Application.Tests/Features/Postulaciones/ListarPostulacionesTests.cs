using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ListarPostulacionesTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ListarPostulaciones _sut;

    public ListarPostulacionesTests()
    {
        _sut = new ListarPostulaciones(_repo);
    }

    [Fact]
    public async Task Consulta_valida_aplica_limites_y_retorna_pagina()
    {
        var paginaEsperada = new PaginaPostulacionesDto(
            Items: new List<PostulacionResumenDto>(),
            Pagina: 1,
            TamanoPagina: 20,
            Total: 0
        );

        _repo.ListarPostulacionesAsync(1, 2, 3, 4, "perez", 1, 20, Arg.Any<CancellationToken>())
            .Returns(paginaEsperada);

        var consulta = new ListarPostulacionesConsulta(
            IdCohorte: 1,
            IdCarrera: 2,
            IdModalidad: 3,
            IdEstado: 4,
            Busqueda: "  perez  ",
            Pagina: 1,
            TamanoPagina: 20
        );

        var resultado = await _sut.EjecutarAsync(consulta);

        resultado.Should().BeEquivalentTo(paginaEsperada);
        await _repo.Received(1).ListarPostulacionesAsync(1, 2, 3, 4, "perez", 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Pagina_menor_a_uno_se_normaliza_a_uno()
    {
        var consulta = new ListarPostulacionesConsulta(Pagina: -5, TamanoPagina: 20);

        await _sut.EjecutarAsync(consulta);

        await _repo.Received(1).ListarPostulacionesAsync(
            null, null, null, null, null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Tamano_pagina_excesivo_se_acota_al_maximo()
    {
        var consulta = new ListarPostulacionesConsulta(Pagina: 1, TamanoPagina: 999);

        await _sut.EjecutarAsync(consulta);

        await _repo.Received(1).ListarPostulacionesAsync(
            null, null, null, null, null, 1, ListarPostulaciones.TamanoPaginaMaximo, Arg.Any<CancellationToken>());
    }
}
