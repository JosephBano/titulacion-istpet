using TitulacionIstpet.Application.DTOs.Egresados;

namespace TitulacionIstpet.Application.Interfaces;

public interface IEgresadosService
{
    Task<PagedResultDto<EstudiantePendienteEgresoDto>> GetPendientesEgresoAsync(
        FiltroPendientesEgresoRequestDto filtro,
        CancellationToken cancellationToken = default);

    Task<ExpedienteAcademicoDto?> GetExpedienteAcademicoAsync(
        string idAlumno,
        int? idCarrera = null,
        CancellationToken cancellationToken = default);
}
