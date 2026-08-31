using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ObtenerPostulacionPorIdTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ObtenerPostulacionPorId _sut;

    public ObtenerPostulacionPorIdTests()
    {
        _sut = new ObtenerPostulacionPorId(_repo);
    }

    [Fact]
    public async Task Consulta_existente_retorna_detalle()
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

        _repo.ObtenerPorIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(detalleEsperado);

        var resultado = await _sut.EjecutarAsync(new ObtenerPostulacionPorIdConsulta(10));

        resultado.Should().BeEquivalentTo(detalleEsperado);
    }

    [Fact]
    public async Task Consulta_inexistente_lanza_NoEncontradoException()
    {
        _repo.ObtenerPorIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((PostulacionDetalleDto?)null);

        var accion = () => _sut.EjecutarAsync(new ObtenerPostulacionPorIdConsulta(999));

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }

    [Fact]
    public async Task Id_invalido_lanza_ArgumentException()
    {
        var accion = () => _sut.EjecutarAsync(new ObtenerPostulacionPorIdConsulta(0));

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador de la postulación*");
    }
}
