using Titan.Domain.Entities;

namespace Titan.Domain.Repositories;

/// <summary>
/// El dominio declara que necesita persistencia; Infrastructure decide cómo.
/// </summary>
public interface IEstudianteRepository
{
    Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Estudiante?> ObtenerPorCedulaAsync(string cedula, CancellationToken ct = default);
    Task<IReadOnlyList<Estudiante>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default);
    void Eliminar(Estudiante estudiante);
}
