using Microsoft.EntityFrameworkCore;
using Titan.Domain.Entities;
using Titan.Domain.Repositories;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Persistence.Repositories;

public class EstudianteRepository : IEstudianteRepository
{
    private readonly TitanDbContext _db;

    public EstudianteRepository(TitanDbContext db) => _db = db;

    public Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Set<Estudiante>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<Estudiante?> ObtenerPorCedulaAsync(string cedula, CancellationToken ct = default) =>
        _db.Set<Estudiante>().FirstOrDefaultAsync(e => e.Cedula == cedula, ct);

    public async Task<IReadOnlyList<Estudiante>> ListarAsync(CancellationToken ct = default) =>
        await _db.Set<Estudiante>().AsNoTracking().OrderBy(e => e.Apellidos).ToListAsync(ct);

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default) =>
        await _db.Set<Estudiante>().AddAsync(estudiante, ct);

    public void Eliminar(Estudiante estudiante) => _db.Set<Estudiante>().Remove(estudiante);
}
