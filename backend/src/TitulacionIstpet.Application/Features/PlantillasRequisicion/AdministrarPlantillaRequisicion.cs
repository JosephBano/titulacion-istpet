using System.Text.Json;
using FluentValidation;
using TitulacionIstpet.Application.Common.Models;

namespace TitulacionIstpet.Application.Features.PlantillasRequisicion;

public sealed class AdministrarPlantillaRequisicion(
    IRepositorioPlantillaRequisicion repositorio,
    IValidator<ActualizarPlantillaRequisicionComandoDto> validador)
{
    public async Task<PlantillaRequisicionEnvelopeDto> ObtenerPlantillaAsync(CancellationToken ct = default)
    {
        var entidad = await repositorio.ObtenerPorCodigoAsync(PlantillaRequisicionBase.CodigoRequisicion, ct);
        if (entidad == null)
        {
            return new PlantillaRequisicionEnvelopeDto(
                Codigo: PlantillaRequisicionBase.CodigoRequisicion,
                Revision: 0,
                EsPredeterminada: true,
                FechaActualizacion: DateTime.UtcNow,
                Definicion: PlantillaRequisicionBase.CrearDefinicionPredeterminada()
            );
        }

        PlantillaRequisicionDefinicionDto? definicion;
        try
        {
            definicion = JsonSerializer.Deserialize<PlantillaRequisicionDefinicionDto>(
                entidad.DefinicionJson,
                PlantillaRequisicionBase.JsonOptions);
        }
        catch
        {
            definicion = null;
        }

        definicion ??= PlantillaRequisicionBase.CrearDefinicionPredeterminada();

        return new PlantillaRequisicionEnvelopeDto(
            Codigo: entidad.Codigo,
            Revision: entidad.Revision,
            EsPredeterminada: false,
            FechaActualizacion: entidad.FechaActualizacion,
            Definicion: definicion
        );
    }

    public async Task<PlantillaRequisicionEnvelopeDto> ActualizarPlantillaAsync(
        ActualizarPlantillaRequisicionComandoDto comando,
        int? usuarioId,
        CancellationToken ct = default)
    {
        var validacion = await validador.ValidateAsync(comando, ct);
        if (!validacion.IsValid)
        {
            throw new ValidacionException(validacion.Errors);
        }

        string json = JsonSerializer.Serialize(comando.Definicion, PlantillaRequisicionBase.JsonOptions);

        var guardada = await repositorio.GuardarOActualizarAsync(
            PlantillaRequisicionBase.CodigoRequisicion,
            json,
            comando.RevisionEsperada,
            usuarioId,
            ct);

        return new PlantillaRequisicionEnvelopeDto(
            Codigo: guardada.Codigo,
            Revision: guardada.Revision,
            EsPredeterminada: false,
            FechaActualizacion: guardada.FechaActualizacion,
            Definicion: comando.Definicion
        );
    }
}
