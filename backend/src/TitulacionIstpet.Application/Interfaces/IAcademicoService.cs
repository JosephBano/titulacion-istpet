using TitulacionIstpet.Application.DTOs.Academico;

namespace TitulacionIstpet.Application.Interfaces;

public interface IAcademicoService
{
    Task<IEnumerable<CarreraResponseDto>> GetCarrerasActivasAsync(CancellationToken cancellationToken = default);
    Task<CarreraResponseDto?> GetCarreraPorIdAsync(int idCarrera, CancellationToken cancellationToken = default);
    Task<IEnumerable<PeriodoResponseDto>> GetPeriodosVigentesAsync(bool soloActivos = false, bool soloInstituto = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<AsignaturaResponseDto>> GetAsignaturasPorMallaAsync(int idCarrera, CancellationToken cancellationToken = default);
    Task<IEnumerable<ModalidadResponseDto>> GetModalidadesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ModalidadCarreraResponseDto>> GetModalidadesCarrerasAsync(bool soloActivas = true, CancellationToken cancellationToken = default);
}
