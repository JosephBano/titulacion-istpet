using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos;

public interface IRepositorioResponsablesRequisitos
{
    Task<IReadOnlyList<ResponsableRequisitoDto>> ListarPorRequisitoAsync(int idRequisito, CancellationToken ct = default);
    Task<IReadOnlyList<ProfesorCandidatoDto>> ListarProfesoresCandidatosAsync(string? busqueda, CancellationToken ct = default);
    Task<int> AsignarProfesorAsync(int idRequisitos, string idProfesor, CancellationToken ct = default);
    Task DesasignarProfesorAsync(int idResponsableEvidencias, CancellationToken ct = default);
    Task<IReadOnlyList<RequisitoEvaluacionDocenteDto>> ListarPendientesDocenteAsync(string idProfesor, CancellationToken ct = default);
    Task EvaluarRequisitoAsync(EvaluarRequisitoDocenteDto comando, string idEvaluador, CancellationToken ct = default);
    Task<IReadOnlyList<EvaluacionDocenteItemDto>> ListarEvaluacionesPorRequisitoPostulacionAsync(int idPostulacionAlumnoRequisitoModalidad, CancellationToken ct = default);
}
