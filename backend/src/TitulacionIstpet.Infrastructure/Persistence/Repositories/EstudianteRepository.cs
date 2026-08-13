using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Repositories;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public class EstudianteRepository : IEstudianteRepository
{
    private readonly AppDbContext _db;

    public EstudianteRepository(AppDbContext db) => _db = db;

    public Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Estudiantes.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<Estudiante?> ObtenerPorCedulaAsync(string cedula, CancellationToken ct = default) =>
        _db.Estudiantes.FirstOrDefaultAsync(e => e.Cedula == cedula, ct);

    public async Task<IReadOnlyList<Estudiante>> ListarAsync(CancellationToken ct = default) =>
        await _db.Estudiantes.AsNoTracking().OrderBy(e => e.Apellidos).ToListAsync(ct);

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default) =>
        await _db.Estudiantes.AddAsync(estudiante, ct);

    public void Eliminar(Estudiante estudiante) => _db.Estudiantes.Remove(estudiante);
}
