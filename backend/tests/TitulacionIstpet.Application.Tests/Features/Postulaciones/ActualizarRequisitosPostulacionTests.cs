using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Comandos;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ActualizarRequisitosPostulacionTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ActualizarRequisitosPostulacion _sut;

    public ActualizarRequisitosPostulacionTests()
    {
        _sut = new ActualizarRequisitosPostulacion(_repo);
    }

    [Fact]
    public async Task Actualizacion_valida_invoca_repositorio()
    {
        var requisitos = new List<RequisitoPostulacionInputDto>
        {
            new(IdRequisitoModalidad: 1, IdAdjuntosImagenes: 15, ValorBool: null),
            new(IdRequisitoModalidad: 2, IdAdjuntosImagenes: null, ValorBool: true)
        };
        var comando = new ActualizarRequisitosPostulacionComando(
            IdPostulacionAlumnos: 10,
            Requisitos: requisitos);

        await _sut.EjecutarAsync(comando);

        await _repo.Received(1).ActualizarRequisitosAsync(
            10,
            Arg.Is<IReadOnlyList<RequisitoPostulacionInputDto>>(r => r.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Id_postulacion_invalido_lanza_ArgumentException()
    {
        var comando = new ActualizarRequisitosPostulacionComando(
            IdPostulacionAlumnos: 0,
            Requisitos: new List<RequisitoPostulacionInputDto>());

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador de la postulación*");
    }

    [Fact]
    public async Task Requisitos_nulo_lanza_ArgumentNullException()
    {
        var comando = new ActualizarRequisitosPostulacionComando(
            IdPostulacionAlumnos: 10,
            Requisitos: null!);

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentNullException>();
    }
}
