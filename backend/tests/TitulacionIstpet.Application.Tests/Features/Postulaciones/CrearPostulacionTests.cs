using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Comandos;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class CrearPostulacionTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly CrearPostulacion _sut;

    public CrearPostulacionTests()
    {
        _sut = new CrearPostulacion(_repo);
    }

    [Fact]
    public async Task Crear_con_datos_validos_invoca_repositorio_y_retorna_id()
    {
        var requisitos = new List<RequisitoPostulacionInputDto>
        {
            new(IdRequisitoModalidad: 1, IdAdjuntosImagenes: 10, ValorBool: null),
            new(IdRequisitoModalidad: 2, IdAdjuntosImagenes: null, ValorBool: true)
        };
        var comando = new CrearPostulacionComando(
            IdMatricula: 500,
            IdModalidadTitulacionCarrera: 12,
            Requisitos: requisitos);

        _repo.CrearPostulacionAsync(500, 12, Arg.Any<IReadOnlyList<RequisitoPostulacionInputDto>>(), Arg.Any<CancellationToken>())
            .Returns(101);

        int idGenerado = await _sut.EjecutarAsync(comando);

        idGenerado.Should().Be(101);
        await _repo.Received(1).CrearPostulacionAsync(500, 12, Arg.Is<IReadOnlyList<RequisitoPostulacionInputDto>>(r => r.Count == 2), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_con_matricula_invalida_lanza_ArgumentException()
    {
        var comando = new CrearPostulacionComando(0, 12, null);

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador de matrícula*");
    }

    [Fact]
    public async Task Crear_con_modalidad_invalida_lanza_ArgumentException()
    {
        var comando = new CrearPostulacionComando(500, 0, null);

        var accion = () => _sut.EjecutarAsync(comando);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*modalidad de titulación*");
    }
}
