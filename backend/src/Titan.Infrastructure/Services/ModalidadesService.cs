using Microsoft.EntityFrameworkCore;
using Titan.Application.DTOs.Academico;
using Titan.Application.Interfaces;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Services;

public class ModalidadesService : IModalidadesService
{
    private readonly TitanDbContext _context;

    public ModalidadesService(TitanDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ModalidadDto>> GetModalidadesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.modalidades
            .AsNoTracking()
            .OrderBy(m => m.idModalidad)
            .Select(m => new ModalidadDto(
                m.idModalidad,
                m.modalidad ?? string.Empty,
                m.modalidadImpresion
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ModalidadDto?> GetModalidadPorIdAsync(int idModalidad, CancellationToken cancellationToken = default)
    {
        return await _context.modalidades
            .AsNoTracking()
            .Where(m => m.idModalidad == idModalidad)
            .Select(m => new ModalidadDto(
                m.idModalidad,
                m.modalidad ?? string.Empty,
                m.modalidadImpresion
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModalidadCarreraDto>> GetModalidadesCarrerasTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.modalidades_carreras
            .AsNoTracking()
            .Include(mc => mc.idCarreraNavigation)
            .Include(mc => mc.idModalidadNavigation)
            .Select(mc => new ModalidadCarreraDto(
                mc.idModalidadCarrera,
                mc.idCarrera,
                mc.idCarreraNavigation != null ? (mc.idCarreraNavigation.Carrera ?? string.Empty) : string.Empty,
                mc.idModalidad,
                mc.idModalidadNavigation != null ? (mc.idModalidadNavigation.modalidad ?? string.Empty) : string.Empty,
                mc.esActivo.HasValue && mc.esActivo.Value == 1
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModalidadCarreraDto>> GetModalidadesPorCarreraAsync(int idCarrera, CancellationToken cancellationToken = default)
    {
        return await _context.modalidades_carreras
            .AsNoTracking()
            .Include(mc => mc.idCarreraNavigation)
            .Include(mc => mc.idModalidadNavigation)
            .Where(mc => mc.idCarrera == idCarrera)
            .Select(mc => new ModalidadCarreraDto(
                mc.idModalidadCarrera,
                mc.idCarrera,
                mc.idCarreraNavigation != null ? (mc.idCarreraNavigation.Carrera ?? string.Empty) : string.Empty,
                mc.idModalidad,
                mc.idModalidadNavigation != null ? (mc.idModalidadNavigation.modalidad ?? string.Empty) : string.Empty,
                mc.esActivo.HasValue && mc.esActivo.Value == 1
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SistemaTitulacionDto>> GetSistemasTitulacionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.sistema_titulacion
            .AsNoTracking()
            .Where(s => s.activo == 1 || s.activo == null)
            .OrderBy(s => s.codigo_sistema)
            .Select(s => new SistemaTitulacionDto(
                s.codigo_sistema,
                s.detalle ?? string.Empty,
                s.activo.HasValue && s.activo.Value == 1
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<EstudianteModalidadContextDto?> GetContextoModalidadesEstudianteAsync(string idAlumno, CancellationToken cancellationToken = default)
    {
        // 1. Obtener la matrícula activa del estudiante para identificar su modalidad y carrera
        var ultimaMatricula = await _context.matriculas
            .AsNoTracking()
            .Include(m => m.idModalidadNavigation)
            .Where(m => m.idAlumno == idAlumno && (m.retirado == null || m.retirado == false))
            .OrderByDescending(m => m.idMatricula)
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Obtener la carrera del alumno desde alumnos_carreras
        var alumnoCarrera = await _context.alumnos_carreras
            .AsNoTracking()
            .Where(ac => ac.idAlumno == idAlumno)
            .FirstOrDefaultAsync(cancellationToken);

        if (alumnoCarrera == null && ultimaMatricula == null)
        {
            return null;
        }

        int idCarrera = alumnoCarrera?.idCarrera ?? 0;
        var carreraEntidad = idCarrera > 0
            ? await _context.carreras.AsNoTracking().FirstOrDefaultAsync(c => c.idCarrera == idCarrera, cancellationToken)
            : null;

        string nombreCarrera = carreraEntidad?.Carrera ?? "Carrera Institucional";

        int idModalidad = ultimaMatricula?.idModalidad ?? 1;
        string nombreModalidad = ultimaMatricula?.idModalidadNavigation?.modalidad ?? "Presencial";

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
