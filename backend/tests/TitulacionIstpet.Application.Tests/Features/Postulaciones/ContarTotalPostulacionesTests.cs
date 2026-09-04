using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ContarTotalPostulacionesTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ContarTotalPostulaciones _sut;

    public ContarTotalPostulacionesTests()
    {
        _sut = new ContarTotalPostulaciones(_repo);
    }

    [Fact]
    public async Task ContarTotalPostulaciones_retorna_dto_con_total_correcto()
    {
        const int conteoEsperado = 42;
        _repo.ContarTotalPostulacionesAsync(Arg.Any<CancellationToken>())
            .Returns(conteoEsperado);

        var resultado = await _sut.EjecutarAsync();

        resultado.Should().NotBeNull();
        resultado.TotalPostulaciones.Should().Be(conteoEsperado);
        await _repo.Received(1).ContarTotalPostulacionesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ContarTotalPostulaciones_cuando_es_cero_retorna_cero()
    {
        _repo.ContarTotalPostulacionesAsync(Arg.Any<CancellationToken>())
            .Returns(0);

        var resultado = await _sut.EjecutarAsync();

        resultado.Should().NotBeNull();
        resultado.TotalPostulaciones.Should().Be(0);
        await _repo.Received(1).ContarTotalPostulacionesAsync(Arg.Any<CancellationToken>());
    }
}
