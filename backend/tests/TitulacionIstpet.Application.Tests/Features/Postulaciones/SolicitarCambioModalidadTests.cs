using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Comandos;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class SolicitarCambioModalidadTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly SolicitarCambioModalidad _sut;

    public SolicitarCambioModalidadTests()
    {
        _sut = new SolicitarCambioModalidad(_repo);
    }

    [Fact]
    public async Task Solicitud_valida_invoca_repositorio()
    {
        var comando = new SolicitarCambioModalidadComando(
            IdPostulacionAlumnos: 15,
            IdNuevaModalidadTitulacionCarrera: 3);

        await _sut.EjecutarAsync(comando);

        await _repo.Received(1).SolicitarCambioModalidadAsync(15, 3, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Id_postulacion_invalido_lanza_ArgumentException()
    {
        var comando = new SolicitarCambioModalidadComando(0, 3);

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador de la postulación*");
    }

    [Fact]
    public async Task Id_nueva_modalidad_invalido_lanza_ArgumentException()
    {
        var comando = new SolicitarCambioModalidadComando(15, 0);

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador de la nueva modalidad*");
    }
}
