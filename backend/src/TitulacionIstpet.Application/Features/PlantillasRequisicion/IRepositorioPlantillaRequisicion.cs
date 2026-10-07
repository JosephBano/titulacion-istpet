using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Features.PlantillasRequisicion;

public interface IRepositorioPlantillaRequisicion
{
    Task<TitulPlantillaDocumento?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);

    Task<TitulPlantillaDocumento> GuardarOActualizarAsync(
        string codigo,
        string definicionJson,
        int revisionEsperada,
        int? usuarioId,
        CancellationToken ct = default);
}
