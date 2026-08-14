using TitulacionIstpet.Application.DTOs.Academico;

namespace TitulacionIstpet.Application.Interfaces;

public interface IModalidadesService
{
    Task<IEnumerable<ModalidadDto>> GetModalidadesAsync(CancellationToken cancellationToken = default);
    Task<ModalidadDto?> GetModalidadPorIdAsync(int idModalidad, CancellationToken cancellationToken = default);
    Task<IEnumerable<ModalidadCarreraDto>> GetModalidadesCarrerasTodasAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ModalidadCarreraDto>> GetModalidadesPorCarreraAsync(int idCarrera, CancellationToken cancellationToken = default);
    Task<EstudianteModalidadContextDto?> GetContextoModalidadesEstudianteAsync(string idAlumno, CancellationToken cancellationToken = default);
    Task<IEnumerable<SistemaTitulacionDto>> GetSistemasTitulacionAsync(CancellationToken cancellationToken = default);
}
