using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Domain.Repositories;

/// <summary>
/// El dominio declara que necesita persistencia; Infrastructure decide como.
/// Esto es lo que mantiene la dependencia apuntando hacia adentro.
/// </summary>
public interface IEstudianteRepository
{
    Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Estudiante?> ObtenerPorCedulaAsync(string cedula, CancellationToken ct = default);
    Task<IReadOnlyList<Estudiante>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default);
    void Eliminar(Estudiante estudiante);
}
