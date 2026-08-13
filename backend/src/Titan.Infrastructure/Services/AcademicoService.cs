using Microsoft.EntityFrameworkCore;
using Titan.Application.DTOs.Academico;
using Titan.Application.Interfaces;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Services;

public class AcademicoService : IAcademicoService
{
    private readonly TitanDbContext _context;

    public AcademicoService(TitanDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CarreraResponseDto>> GetCarrerasActivasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.carreras
            .AsNoTracking()
            .Where(c => c.activa == true)
            .OrderBy(c => c.Carrera)
            .Select(c => new CarreraResponseDto(
                c.idCarrera,
                c.Carrera ?? string.Empty,
                c.aliasCarrera ?? string.Empty,
                c.codigo_cases ?? string.Empty,
                c.directorCarrera ?? string.Empty,
                c.activa ?? false
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<CarreraResponseDto?> GetCarreraPorIdAsync(int idCarrera, CancellationToken cancellationToken = default)
    {
        return await _context.carreras
            .AsNoTracking()
            .Where(c => c.idCarrera == idCarrera)
            .Select(c => new CarreraResponseDto(
                c.idCarrera,
                c.Carrera ?? string.Empty,
                c.aliasCarrera ?? string.Empty,
                c.codigo_cases ?? string.Empty,
                c.directorCarrera ?? string.Empty,
                c.activa ?? false
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<PeriodoResponseDto>> GetPeriodosVigentesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.periodos
            .AsNoTracking()
            .Where(p => p.activo == true)
            .OrderByDescending(p => p.idPeriodo)
            .Select(p => new PeriodoResponseDto(
                p.idPeriodo,
                p.detalle ?? p.idPeriodo,
                p.fecha_inicial,
                p.fecha_final,
                p.activo ?? false
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AsignaturaResponseDto>> GetAsignaturasPorMallaAsync(int idCarrera, CancellationToken cancellationToken = default)
    {
        return await _context.detallemallas
            .AsNoTracking()
            .Include(dm => dm.idMallaNavigation)
            .Include(dm => dm.idAsignaturaNavigation)
            .Where(dm => dm.idMallaNavigation.idCarrera == idCarrera && dm.idAsignaturaNavigation != null)
            .Select(dm => new AsignaturaResponseDto(
                dm.idAsignaturaNavigation.idAsignatura,
                dm.idAsignaturaNavigation.asignatura ?? string.Empty,
                dm.creditos,
                dm.horas,
                dm.idNivel
            ))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModalidadResponseDto>> GetModalidadesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.modalidades
            .AsNoTracking()
            .OrderBy(m => m.idModalidad)
            .Select(m => new ModalidadResponseDto(
                m.idModalidad,
                m.modalidad ?? string.Empty
            ))
            .ToListAsync(cancellationToken);
    }
}
