using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Comandos;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class CambiarEstadoPostulacionTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly CambiarEstadoPostulacion _sut;

    public CambiarEstadoPostulacionTests()
    {
        _sut = new CambiarEstadoPostulacion(_repo);
    }

    [Fact]
    public async Task Cambio_valido_invoca_repositorio()
    {
        var comando = new CambiarEstadoPostulacionComando(IdPostulacionAlumnos: 25, IdNuevoEstado: 3);

        await _sut.EjecutarAsync(comando);

        await _repo.Received(1).CambiarEstadoAsync(25, 3, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Id_postulacion_invalido_lanza_ArgumentException()
    {
        var comando = new CambiarEstadoPostulacionComando(0, 3);

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador de la postulación*");
    }

    [Fact]
    public async Task Id_estado_invalido_lanza_ArgumentException()
    {
        var comando = new CambiarEstadoPostulacionComando(25, 0);

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador del nuevo estado*");
    }
}
