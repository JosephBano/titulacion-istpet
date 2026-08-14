using TitulacionIstpet.Application.DTOs.Actores;

namespace TitulacionIstpet.Application.Interfaces;

public interface IActoresService
{
    Task<IEnumerable<AlumnoResponseDto>> BuscarAlumnosAsync(string? busqueda, CancellationToken cancellationToken = default);
    Task<AlumnoResponseDto?> GetAlumnoPorCedulaAsync(string cedula, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProfesorResponseDto>> GetDocentesEvaluadoresAsync(CancellationToken cancellationToken = default);
    Task<ProfesorResponseDto?> GetDocentePorCedulaAsync(string cedula, CancellationToken cancellationToken = default);
    Task<IEnumerable<MatriculaResponseDto>> GetMatriculasPorAlumnoAsync(string idAlumno, CancellationToken cancellationToken = default);
    Task<AptitudTitulacionResponseDto> ValidarAptitudTitulacionAsync(string idAlumno, int idCarrera, CancellationToken cancellationToken = default);
    Task<IEnumerable<AlumnoAptoDto>> GetAlumnosAptosTitulacionAsync(int? idCarrera, int? idModalidad, string? busqueda, CancellationToken cancellationToken = default);
    Task<IEnumerable<GraduadoHistoricoDto>> GetAlumnosGraduadosAsync(int? idCarrera, string? busqueda, CancellationToken cancellationToken = default);
}
