using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ObtenerMiPostulacionTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ObtenerMiPostulacion _sut;

    public ObtenerMiPostulacionTests()
    {
        _sut = new ObtenerMiPostulacion(_repo);
    }

    [Fact]
    public async Task Consulta_valida_retorna_detalle_de_postulacion()
    {
        var detalleEsperado = new PostulacionDetalleDto(
            IdPostulacionAlumnos: 10,
            IdMatricula: 100,
            IdAlumno: "1712345678",
            NombreAlumno: "Carlos Mora",
            CedulaAlumno: "1712345678",
            EmailAlumno: "carlos@istpet.edu.ec",
            TelefonoAlumno: "0999999999",
            IdCarrera: 3,
            NombreCarrera: "Desarrollo de Software",
            IdCohorte: 1,
            DetalleCohorte: "2026-I",
            IdModalidadTitulacionCarrera: 2,
            ModalidadTitulacion: "Examen Complexivo",
            IdPostulacionEstado: 1,
            NombreEstado: "Postulado",
            EsActivo: true,
            EsCambioModalidad: false,
            Requisitos: new List<PostulacionRequisitoDetalleDto>()
        );

        _repo.ObtenerMiPostulacionActivaAsync("1712345678", Arg.Any<CancellationToken>())
            .Returns(detalleEsperado);

        var resultado = await _sut.EjecutarAsync(new ObtenerMiPostulacionConsulta("  1712345678  "));

        resultado.Should().BeEquivalentTo(detalleEsperado);
        await _repo.Received(1).ObtenerMiPostulacionActivaAsync("1712345678", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Consulta_con_identificador_vacio_lanza_ArgumentException(string? idInvalido)
    {
        var accion = () => _sut.EjecutarAsync(new ObtenerMiPostulacionConsulta(idInvalido!));

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador del estudiante*");
    }
}
