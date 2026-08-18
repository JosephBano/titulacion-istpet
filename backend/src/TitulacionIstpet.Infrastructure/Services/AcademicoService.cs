using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Academico;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class AcademicoService : IAcademicoService
{
    private readonly SigafiDbContext _context;

    public AcademicoService(SigafiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CarreraResponseDto>> GetCarrerasActivasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Carreras
            .AsNoTracking()
            .Where(c => c.Activa == true)
            .OrderBy(c => c.Carrera)
            .Select(c => new CarreraResponseDto(
                c.IdCarrera,
                c.Carrera ?? string.Empty,
                c.AliasCarrera ?? string.Empty,
                c.CodigoCases ?? string.Empty,
                c.DirectorCarrera ?? string.Empty,
                c.Activa ?? false
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<CarreraResponseDto?> GetCarreraPorIdAsync(int idCarrera, CancellationToken cancellationToken = default)
    {
        return await _context.Carreras
            .AsNoTracking()
            .Where(c => c.IdCarrera == idCarrera)
            .Select(c => new CarreraResponseDto(
                c.IdCarrera,
                c.Carrera ?? string.Empty,
                c.AliasCarrera ?? string.Empty,
                c.CodigoCases ?? string.Empty,
                c.DirectorCarrera ?? string.Empty,
                c.Activa ?? false
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<PeriodoResponseDto>> GetPeriodosVigentesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Periodos
            .AsNoTracking()
            .Where(p => p.Activo == true)
            .OrderByDescending(p => p.IdPeriodo)
            .Select(p => new PeriodoResponseDto(
                p.IdPeriodo,
                p.Detalle ?? p.IdPeriodo,
                p.FechaInicial,
                p.FechaFinal,
                p.Activo ?? false
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AsignaturaResponseDto>> GetAsignaturasPorMallaAsync(int idCarrera, CancellationToken cancellationToken = default)
    {
        return await _context.Detallemallas
            .AsNoTracking()
            .Include(dm => dm.IdMallaNavigation)
            .Include(dm => dm.IdAsignaturaNavigation)
            .Where(dm => dm.IdMallaNavigation.IdCarrera == idCarrera && dm.IdAsignaturaNavigation != null)
            .Select(dm => new AsignaturaResponseDto(
                dm.IdAsignaturaNavigation.IdAsignatura,
                dm.IdAsignaturaNavigation.Asignatura ?? string.Empty,
                dm.Creditos,
                dm.Horas,
                dm.IdNivel
            ))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModalidadResponseDto>> GetModalidadesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Modalidades
            .AsNoTracking()
            .OrderBy(m => m.IdModalidad)
            .Select(m => new ModalidadResponseDto(
                m.IdModalidad,
                m.Modalidad ?? string.Empty
            ))
            .ToListAsync(cancellationToken);
    }
}
