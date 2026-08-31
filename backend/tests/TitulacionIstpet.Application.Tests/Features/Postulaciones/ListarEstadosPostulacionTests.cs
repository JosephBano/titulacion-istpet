using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ListarEstadosPostulacionTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ListarEstadosPostulacion _sut;

    public ListarEstadosPostulacionTests()
    {
        _sut = new ListarEstadosPostulacion(_repo);
    }

    [Fact]
    public async Task ListarEstados_retorna_catalogo_de_estados()
    {
        var estadosEsperados = new List<EstadoPostulacionDto>
        {
            new(1, "Postulado", 1, false, true),
            new(2, "Aprobado", 2, true, true)
        };

        _repo.ListarEstadosAsync(Arg.Any<CancellationToken>())
            .Returns(estadosEsperados);

        var resultado = await _sut.EjecutarAsync();

        resultado.Should().BeEquivalentTo(estadosEsperados);
        await _repo.Received(1).ListarEstadosAsync(Arg.Any<CancellationToken>());
    }
}
