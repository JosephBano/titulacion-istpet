using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementacion con EF Core. Toma el <see cref="SigafiDbContext"/> por DI,
/// que ya viene scoped por peticion, asi que esta clase y el caso de uso que
/// recibe <see cref="IUnitOfWork"/> operan sobre el mismo ChangeTracker y
/// la misma transaccion.
/// </summary>
public sealed class RepositorioAdjuntosImagenes : IRepositorioAdjuntosImagenes
{
    private readonly SigafiDbContext _db;

    public RepositorioAdjuntosImagenes(SigafiDbContext db)
    {
        _db = db;
    }

    public Task<AdjuntosImagene?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.AdjuntosImagenes.FirstOrDefaultAsync(e => e.IdAdjuntosImagenes == id, ct);

    public async Task<IReadOnlyList<AdjuntosImagene>> ListarAsync(
        int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var salto = (pagina - 1) * tamanoPagina;

        return await _db.AdjuntosImagenes
            .AsNoTracking()
            .OrderBy(e => e.IdAdjuntosImagenes)
            .Skip(salto)
            .Take(tamanoPagina)
            .ToListAsync(ct);
    }

    public Task<int> ContarAsync(CancellationToken ct = default) =>
        _db.AdjuntosImagenes.CountAsync(ct);

    public void Agregar(AdjuntosImagene entidad) =>
        _db.AdjuntosImagenes.Add(entidad);

    public void Actualizar(AdjuntosImagene entidad) =>
        _db.AdjuntosImagenes.Update(entidad);

    public void Eliminar(AdjuntosImagene entidad) =>
        _db.AdjuntosImagenes.Remove(entidad);
}
