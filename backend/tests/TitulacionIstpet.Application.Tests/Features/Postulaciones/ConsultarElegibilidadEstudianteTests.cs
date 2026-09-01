using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ConsultarElegibilidadEstudianteTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ConsultarElegibilidadEstudiante _sut;

    public ConsultarElegibilidadEstudianteTests()
    {
        _sut = new ConsultarElegibilidadEstudiante(_repo);
    }

    [Fact]
    public async Task Consulta_valida_invoca_repositorio_recortando_espacios()
    {
        var retornoEsperado = new ElegibilidadPostulacionDto(
            EsElegible: true,
            Mensaje: "Habilitado",
            IdMatricula: 1,
            IdAlumno: "1712345678",
            NombreCompleto: "Juan Perez",
            IdCarrera: 5,
            NombreCarrera: "Desarrollo de Software",
            IdCohorte: 2,
            DetalleCohorte: "2026-I",
            TienePostulacionActiva: false,
            IdPostulacionActiva: null,
            EstadoPostulacionActiva: null,
            ModalidadesOfertadas: new List<ModalidadOfertadaDto>()
        );

        _repo.ObtenerElegibilidadEstudianteAsync("1712345678", Arg.Any<CancellationToken>())
            .Returns(retornoEsperado);

        var resultado = await _sut.EjecutarAsync(new ConsultarElegibilidadEstudianteConsulta("  1712345678  "));

        resultado.Should().BeEquivalentTo(retornoEsperado);
        await _repo.Received(1).ObtenerElegibilidadEstudianteAsync("1712345678", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Consulta_con_identificador_vacio_lanza_ArgumentException(string? idInvalido)
    {
        var accion = () => _sut.EjecutarAsync(new ConsultarElegibilidadEstudianteConsulta(idInvalido!));

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador del estudiante*");
    }
}
