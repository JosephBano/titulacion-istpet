using TitulacionIstpet.Application.DTOs.Academico;

namespace TitulacionIstpet.Application.Interfaces;

public interface ICarrerasService
{
    Task<IEnumerable<CarreraDto>> GetCarrerasTodasAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EstudianteCarreraDto>> GetCarrerasPorEstudianteAsync(string idAlumno, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProfesorCarreraDto>> GetCarrerasPorProfesorAsync(string idProfesor, CancellationToken cancellationToken = default);
    Task<UsuarioCarrerasResponseDto?> GetCarrerasUsuarioAutenticadoAsync(string idSigafi, CancellationToken cancellationToken = default);
}
