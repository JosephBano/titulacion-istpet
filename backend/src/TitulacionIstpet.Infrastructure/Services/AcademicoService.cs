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

    public async Task<IEnumerable<PeriodoResponseDto>> GetPeriodosVigentesAsync(
        bool soloActivos = false,
        bool soloInstituto = true,
        bool soloVigentesOFuturos = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Periodos.AsNoTracking().AsQueryable();

        if (soloInstituto)
        {
            query = query.Where(p => p.IdPeriodo.StartsWith("ABR") || p.IdPeriodo.StartsWith("OCT") || p.EsInstituto == true || p.Periodoactivoinstituto == true);
        }

        if (soloActivos)
        {
            query = query.Where(p => p.Activo == true || p.Periodoactivoinstituto == true);
        }

        if (soloVigentesOFuturos)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(p =>
                p.Activo == true
                || p.Periodoactivoinstituto == true
                || p.PeriodoPlanificacion == true
                || (p.FechaFinal != null && p.FechaFinal >= today.AddMonths(-2))
                || (p.FechaInicial != null && p.FechaInicial >= today)
                || (p.Cerrado != true && (p.Activo == true || p.PeriodoPlanificacion == true))
            );
        }

        return await query
            .OrderByDescending(p => p.IdPeriodo)
            .Select(p => new PeriodoResponseDto(
                p.IdPeriodo,
                p.Detalle ?? p.IdPeriodo,
                p.FechaInicial,
                p.FechaFinal,
                (p.Activo == true || p.Periodoactivoinstituto == true)
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

    public async Task<IEnumerable<ModalidadCarreraResponseDto>> GetModalidadesCarrerasAsync(
        bool soloActivas = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ModalidadesCarreras
            .AsNoTracking()
            .Include(mc => mc.IdCarreraNavigation)
            .Include(mc => mc.IdModalidadNavigation)
            .AsQueryable();

        if (soloActivas)
        {
            query = query.Where(mc =>
                (mc.EsActivo == true || mc.EsActivo == null) &&
                (mc.IdCarreraNavigation.Activa == true || mc.IdCarreraNavigation.Activa == null));
        }

        return await query
            .OrderBy(mc => mc.IdCarreraNavigation.Carrera)
            .ThenBy(mc => mc.IdModalidadNavigation.Modalidad)
            .Select(mc => new ModalidadCarreraResponseDto(
                mc.IdModalidadCarrera,
                mc.IdCarrera,
                mc.IdCarreraNavigation.Carrera ?? string.Empty,
                mc.IdCarreraNavigation.AliasCarrera,
                mc.IdModalidad,
                mc.IdModalidadNavigation.Modalidad ?? string.Empty,
                (mc.EsActivo ?? true) && (mc.IdCarreraNavigation.Activa ?? true)
            ))
            .ToListAsync(cancellationToken);
    }
}
