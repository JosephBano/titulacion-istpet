using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Tests.Features.AdjuntosImagenes;

public class ObtenerAdjuntoPorIdTests
{
    private readonly IRepositorioAdjuntosImagenes _repo = Substitute.For<IRepositorioAdjuntosImagenes>();
    private readonly ObtenerAdjuntoPorId _sut;

    public ObtenerAdjuntoPorIdTests()
    {
        _sut = new ObtenerAdjuntoPorId(_repo);
    }

    [Fact]
    public async Task Obtener_con_id_existente_devuelve_dto_sin_navegaciones()
    {
        var entidad = new AdjuntosImagene
        {
            IdAdjuntosImagenes = 5,
            NombreArchivos = "foto.png",
            Extension = "png",
            MimeTypes = "image/png",
            TamanioBytes = 4096,
            Ruta = "uploads/foto.png",
            // Las colecciones de navegacion se quedan en null por defecto; el
            // test verifica que el mapeo no las expone en el DTO.
            CarrerasAdjuntos = new List<CarrerasAdjunto> { new() }
        };
        _repo.ObtenerPorIdAsync(5, Arg.Any<CancellationToken>()).Returns(entidad);

        var dto = await _sut.EjecutarAsync(new ObtenerAdjuntoPorIdConsulta(5));

        dto.Should().BeEquivalentTo(new
        {
            IdAdjuntosImagenes = 5,
            NombreArchivos = "foto.png",
            Extension = "png",
            MimeTypes = "image/png",
            TamanioBytes = 4096,
            Ruta = "uploads/foto.png"
        });
    }

    [Fact]
    public async Task Obtener_con_id_inexistente_lanza_NoEncontradoException()
    {
        _repo.ObtenerPorIdAsync(404, Arg.Any<CancellationToken>()).Returns((AdjuntosImagene?)null);

        var accion = () => _sut.EjecutarAsync(new ObtenerAdjuntoPorIdConsulta(404));

        await accion.Should().ThrowAsync<NoEncontradoException>();
    }
}
