using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.PlantillasRequisicion;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.PlantillasRequisicion;

public class AdministrarPlantillaRequisicionTests
{
    private readonly IRepositorioPlantillaRequisicion _repo = Substitute.For<IRepositorioPlantillaRequisicion>();
    private readonly ActualizarPlantillaRequisicionValidator _validator = new();
    private readonly AdministrarPlantillaRequisicion _sut;

    public AdministrarPlantillaRequisicionTests()
    {
        _sut = new AdministrarPlantillaRequisicion(_repo, _validator);
    }

    [Fact]
    public async Task Obtener_sin_registro_retorna_predeterminada_con_revision_cero()
    {
        _repo.ObtenerPorCodigoAsync("requisicion-personal", Arg.Any<CancellationToken>())
            .Returns((TitulPlantillaDocumento?)null);

        var envelope = await _sut.ObtenerPlantillaAsync();

        envelope.EsPredeterminada.Should().BeTrue();
        envelope.Revision.Should().Be(0);
        envelope.Codigo.Should().Be("requisicion-personal");
        envelope.Definicion.Should().NotBeNull();
        envelope.Definicion.Documento.Titulo.Should().Be("FORMATO DE REQUISICIÓN DE PERSONAL");
    }

    [Fact]
    public async Task Obtener_con_registro_retorna_datos_guardados()
    {
        var definicion = PlantillaRequisicionBase.CrearDefinicionPredeterminada();
        var json = JsonSerializer.Serialize(definicion, PlantillaRequisicionBase.JsonOptions);
        var entidad = new TitulPlantillaDocumento
        {
            IdPlantillaDocumento = 1,
            Codigo = "requisicion-personal",
            DefinicionJson = json,
            Revision = 3,
            FechaCreacion = DateTime.UtcNow.AddDays(-1),
            FechaActualizacion = DateTime.UtcNow
        };

        _repo.ObtenerPorCodigoAsync("requisicion-personal", Arg.Any<CancellationToken>())
            .Returns(entidad);

        var envelope = await _sut.ObtenerPlantillaAsync();

        envelope.EsPredeterminada.Should().BeFalse();
        envelope.Revision.Should().Be(3);
        envelope.Codigo.Should().Be("requisicion-personal");
        envelope.Definicion.Should().NotBeNull();
    }

    [Fact]
    public async Task Actualizar_valida_y_actualiza_con_revision()
    {
        var comando = new ActualizarPlantillaRequisicionComandoDto(
            RevisionEsperada: 1,
            Definicion: PlantillaRequisicionBase.CrearDefinicionPredeterminada()
        );

        var guardada = new TitulPlantillaDocumento
        {
            IdPlantillaDocumento = 1,
            Codigo = "requisicion-personal",
            DefinicionJson = "{}",
            Revision = 2,
            ActualizadoPor = 42,
            FechaCreacion = DateTime.UtcNow.AddDays(-1),
            FechaActualizacion = DateTime.UtcNow
        };

        _repo.GuardarOActualizarAsync(
                "requisicion-personal",
                Arg.Any<string>(),
                1,
                42,
                Arg.Any<CancellationToken>())
            .Returns(guardada);

        var resultado = await _sut.ActualizarPlantillaAsync(comando, 42);

        resultado.Revision.Should().Be(2);
        resultado.EsPredeterminada.Should().BeFalse();
        resultado.Codigo.Should().Be("requisicion-personal");
    }

    [Fact]
    public async Task Actualizar_con_datos_invalidos_lanza_ValidacionException()
    {
        var comando = new ActualizarPlantillaRequisicionComandoDto(
            RevisionEsperada: -1,
            Definicion: PlantillaRequisicionBase.CrearDefinicionPredeterminada()
        );

        var accion = () => _sut.ActualizarPlantillaAsync(comando, 42);

        await accion.Should().ThrowAsync<ValidacionException>();
    }

    [Fact]
    public async Task Actualizar_con_conflicto_de_concurrencia_lanza_DominioException()
    {
        var comando = new ActualizarPlantillaRequisicionComandoDto(
            RevisionEsperada: 1,
            Definicion: PlantillaRequisicionBase.CrearDefinicionPredeterminada()
        );

        _repo.GuardarOActualizarAsync(
                "requisicion-personal",
                Arg.Any<string>(),
                1,
                42,
                Arg.Any<CancellationToken>())
            .Returns<TitulPlantillaDocumento>(_ => throw new DominioException("Conflicto de concurrencia: la plantilla fue modificada."));

        var accion = () => _sut.ActualizarPlantillaAsync(comando, 42);

        await accion.Should().ThrowAsync<DominioException>();
    }
}
