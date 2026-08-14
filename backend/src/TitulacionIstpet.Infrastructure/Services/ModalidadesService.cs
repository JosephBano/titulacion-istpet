using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Academico;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class ModalidadesService : IModalidadesService
{
    private readonly SigafiDbContext _context;

    public ModalidadesService(SigafiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ModalidadDto>> GetModalidadesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Modalidades
            .AsNoTracking()
            .OrderBy(m => m.IdModalidad)
            .Select(m => new ModalidadDto(
                m.IdModalidad,
                m.Modalidad ?? string.Empty,
                m.ModalidadImpresion
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ModalidadDto?> GetModalidadPorIdAsync(int idModalidad, CancellationToken cancellationToken = default)
    {
        return await _context.Modalidades
            .AsNoTracking()
            .Where(m => m.IdModalidad == idModalidad)
            .Select(m => new ModalidadDto(
                m.IdModalidad,
                m.Modalidad ?? string.Empty,
                m.ModalidadImpresion
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModalidadCarreraDto>> GetModalidadesCarrerasTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ModalidadesCarreras
            .AsNoTracking()
            .Include(mc => mc.IdCarreraNavigation)
            .Include(mc => mc.IdModalidadNavigation)
            .Select(mc => new ModalidadCarreraDto(
                mc.IdModalidadCarrera,
                mc.IdCarrera,
                mc.IdCarreraNavigation != null ? (mc.IdCarreraNavigation.Carrera1 ?? string.Empty) : string.Empty,
                mc.IdModalidad,
                mc.IdModalidadNavigation != null ? (mc.IdModalidadNavigation.Modalidad ?? string.Empty) : string.Empty,
                mc.EsActivo == true
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModalidadCarreraDto>> GetModalidadesPorCarreraAsync(int idCarrera, CancellationToken cancellationToken = default)
    {
        return await _context.ModalidadesCarreras
            .AsNoTracking()
            .Include(mc => mc.IdCarreraNavigation)
            .Include(mc => mc.IdModalidadNavigation)
            .Where(mc => mc.IdCarrera == idCarrera)
            .Select(mc => new ModalidadCarreraDto(
                mc.IdModalidadCarrera,
                mc.IdCarrera,
                mc.IdCarreraNavigation != null ? (mc.IdCarreraNavigation.Carrera1 ?? string.Empty) : string.Empty,
                mc.IdModalidad,
                mc.IdModalidadNavigation != null ? (mc.IdModalidadNavigation.Modalidad ?? string.Empty) : string.Empty,
                mc.EsActivo == true
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SistemaTitulacionDto>> GetSistemasTitulacionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SistemaTitulacions
            .AsNoTracking()
            .Where(s => s.Activo == true || s.Activo == null)
            .OrderBy(s => s.CodigoSistema)
            .Select(s => new SistemaTitulacionDto(
                s.CodigoSistema,
                s.Detalle ?? string.Empty,
                s.Activo == true
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<EstudianteModalidadContextDto?> GetContextoModalidadesEstudianteAsync(string idAlumno, CancellationToken cancellationToken = default)
    {
        // 1. Obtener la matrícula activa del estudiante para identificar su modalidad y carrera
        var ultimaMatricula = await _context.Matriculas
            .AsNoTracking()
            .Include(m => m.IdModalidadNavigation)
            .Where(m => m.IdAlumno == idAlumno && (m.Retirado == null || m.Retirado == false))
            .OrderByDescending(m => m.IdMatricula)
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Obtener la carrera del alumno desde alumnos_carreras
        var alumnoCarrera = await _context.AlumnosCarreras
            .AsNoTracking()
            .Where(ac => ac.IdAlumno == idAlumno)
            .FirstOrDefaultAsync(cancellationToken);

        if (alumnoCarrera == null && ultimaMatricula == null)
        {
            return null;
        }

        int idCarrera = alumnoCarrera?.IdCarrera ?? 0;
        var carreraEntidad = idCarrera > 0
            ? await _context.Carreras.AsNoTracking().FirstOrDefaultAsync(c => c.IdCarrera == idCarrera, cancellationToken)
            : null;

        string nombreCarrera = carreraEntidad?.Carrera1 ?? "Carrera Institucional";

        int idModalidad = ultimaMatricula?.IdModalidad ?? 1;
        string nombreModalidad = ultimaMatricula?.IdModalidadNavigation?.Modalidad ?? "Presencial";

        // 3. Obtener todas las modalidades asociadas a la carrera del alumno
        var modalidadesCarrera = await GetModalidadesPorCarreraAsync(idCarrera, cancellationToken);
        var modalidadesDtos = modalidadesCarrera.Select(mc => new ModalidadDto(mc.IdModalidad, mc.Modalidad, null));

        // 4. Obtener los mecanismos de titulación activos en el instituto
        var opcionesTitulacion = await GetSistemasTitulacionAsync(cancellationToken);

        return new EstudianteModalidadContextDto(
            idCarrera,
            nombreCarrera,
            idModalidad,
            nombreModalidad,
            modalidadesDtos,
            opcionesTitulacion
        );
    }
}
