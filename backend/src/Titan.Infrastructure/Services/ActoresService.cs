using Microsoft.EntityFrameworkCore;
using Titan.Application.DTOs.Actores;
using Titan.Application.Interfaces;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Services;

public class ActoresService : IActoresService
{
    private readonly TitanDbContext _context;

    public ActoresService(TitanDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AlumnoResponseDto>> BuscarAlumnosAsync(string? busqueda, CancellationToken cancellationToken = default)
    {
        var query = _context.alumnos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(a =>
                a.idAlumno.ToLower().Contains(term) ||
                (a.primerNombre + " " + a.segundoNombre + " " + a.apellidoPaterno + " " + a.apellidoMaterno).ToLower().Contains(term) ||
                (a.email_institucional != null && a.email_institucional.ToLower().Contains(term))
            );
        }

        return await query
            .Take(50)
            .Select(a => new AlumnoResponseDto(
                a.idAlumno,
                $"{a.primerNombre} {a.segundoNombre} {a.apellidoPaterno} {a.apellidoMaterno}".Replace("  ", " ").Trim(),
                a.primerNombre ?? string.Empty,
                a.segundoNombre ?? string.Empty,
                a.apellidoPaterno ?? string.Empty,
                a.apellidoMaterno ?? string.Empty,
                a.email_institucional ?? string.Empty,
                a.email ?? string.Empty,
                a.telefono ?? string.Empty,
                a.celular ?? string.Empty,
                a.direccion ?? string.Empty
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AlumnoResponseDto?> GetAlumnoPorCedulaAsync(string cedula, CancellationToken cancellationToken = default)
    {
        return await _context.alumnos
            .AsNoTracking()
            .Where(a => a.idAlumno == cedula)
            .Select(a => new AlumnoResponseDto(
                a.idAlumno,
                $"{a.primerNombre} {a.segundoNombre} {a.apellidoPaterno} {a.apellidoMaterno}".Replace("  ", " ").Trim(),
                a.primerNombre ?? string.Empty,
                a.segundoNombre ?? string.Empty,
                a.apellidoPaterno ?? string.Empty,
                a.apellidoMaterno ?? string.Empty,
                a.email_institucional ?? string.Empty,
                a.email ?? string.Empty,
                a.telefono ?? string.Empty,
                a.celular ?? string.Empty,
                a.direccion ?? string.Empty
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProfesorResponseDto>> GetDocentesEvaluadoresAsync(CancellationToken cancellationToken = default)
    {
        return await _context.profesores
            .AsNoTracking()
            .Where(p => p.activo == 1)
            .OrderBy(p => p.apellidos)
            .Select(p => new ProfesorResponseDto(
                p.idProfesor,
                $"{p.abreviatura} {p.nombres} {p.apellidos}".Trim(),
                p.nombres ?? string.Empty,
                p.apellidos ?? string.Empty,
                p.titulo ?? string.Empty,
                p.abreviatura ?? string.Empty,
                p.emailInstitucional ?? string.Empty,
                p.celular ?? string.Empty,
                p.activo == 1
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProfesorResponseDto?> GetDocentePorCedulaAsync(string cedula, CancellationToken cancellationToken = default)
    {
        return await _context.profesores
            .AsNoTracking()
            .Where(p => p.idProfesor == cedula)
            .Select(p => new ProfesorResponseDto(
                p.idProfesor,
                $"{p.abreviatura} {p.nombres} {p.apellidos}".Trim(),
                p.nombres ?? string.Empty,
                p.apellidos ?? string.Empty,
                p.titulo ?? string.Empty,
                p.abreviatura ?? string.Empty,
                p.emailInstitucional ?? string.Empty,
                p.celular ?? string.Empty,
                p.activo == 1
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<MatriculaResponseDto>> GetMatriculasPorAlumnoAsync(string idAlumno, CancellationToken cancellationToken = default)
    {
        return await _context.matriculas
            .AsNoTracking()
            .Include(m => m.idAlumnoNavigation)
            .Include(m => m.idPeriodoNavigation)
            .Include(m => m.idModalidadNavigation)
            .Where(m => m.idAlumno == idAlumno && (m.valida == 1 || m.valida == null))
            .OrderByDescending(m => m.idMatricula)
            .Select(m => new MatriculaResponseDto(
                m.idMatricula,
                m.idAlumno,
                $"{m.idAlumnoNavigation.primerNombre} {m.idAlumnoNavigation.apellidoPaterno}".Trim(),
                null,
                string.Empty,
                m.idPeriodo,
                m.idNivel,
                m.idModalidad,
                m.idModalidadNavigation != null ? m.idModalidadNavigation.modalidad : string.Empty
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AptitudTitulacionResponseDto> ValidarAptitudTitulacionAsync(string idAlumno, int idCarrera, CancellationToken cancellationToken = default)
    {
        var alumno = await _context.alumnos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.idAlumno == idAlumno, cancellationToken);

        if (alumno == null)
        {
            return new AptitudTitulacionResponseDto(
                idAlumno,
                "Desconocido",
                "N/A",
                "N/A",
                false,
                false,
                "El alumno especificado no existe en el sistema."
            );
        }

        var carrera = await _context.carreras
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.idCarrera == idCarrera, cancellationToken);

        var ultimaMatricula = await _context.matriculas
            .AsNoTracking()
            .Include(m => m.idPeriodoNavigation)
            .Where(m => m.idAlumno == idAlumno && (m.retirado == false || m.retirado == null))
            .OrderByDescending(m => m.idMatricula)
            .FirstOrDefaultAsync(cancellationToken);

        bool tieneMatricula = ultimaMatricula != null;
        bool esApto = tieneMatricula;

        string mensaje = esApto
            ? "El estudiante se encuentra registrado y apto para iniciar el proceso de postulación a titulación."
            : "El estudiante no registra una matrícula válida en el sistema para la carrera seleccionada.";

        return new AptitudTitulacionResponseDto(
            idAlumno,
            $"{alumno.primerNombre} {alumno.segundoNombre} {alumno.apellidoPaterno} {alumno.apellidoMaterno}".Replace("  ", " ").Trim(),
            carrera?.Carrera ?? "N/A",
            ultimaMatricula?.idPeriodo ?? "N/A",
            tieneMatricula,
            esApto,
            mensaje
        );
    }

    public async Task<IEnumerable<AlumnoAptoDto>> GetAlumnosAptosTitulacionAsync(int? idCarrera, int? idModalidad, string? busqueda, CancellationToken cancellationToken = default)
    {
        var query = _context.matriculas
            .AsNoTracking()
            .Include(m => m.idAlumnoNavigation)
            .Include(m => m.idModalidadNavigation)
            .Where(m => !_context.alumnos_titulos.Any(at =>
                at.idAlumno == m.idAlumno &&
                _context.titulos.Any(t => t.idTitulo == at.idTitulo && _context.alumnos_carreras.Any(ac => ac.idAlumno == m.idAlumno && ac.idCarrera == t.idCarrera))))
            .AsQueryable();

        if (idCarrera.HasValue && idCarrera.Value > 0)
        {
            query = query.Where(m => _context.detallemallas.Any(dm => dm.idAsignaturaNavigation != null && dm.idMallaNavigation.idCarrera == idCarrera.Value));
        }

        if (idModalidad.HasValue && idModalidad.Value > 0)
        {
            query = query.Where(m => m.idModalidad == idModalidad.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(m =>
                m.idAlumno.ToLower().Contains(term) ||
                (m.idAlumnoNavigation != null &&
                    (m.idAlumnoNavigation.primerNombre + " " + m.idAlumnoNavigation.segundoNombre + " " + m.idAlumnoNavigation.apellidoPaterno + " " + m.idAlumnoNavigation.apellidoMaterno).ToLower().Contains(term))
            );
        }

        var result = await query
            .GroupBy(m => m.idAlumno)
            .Select(g => g.OrderByDescending(m => m.idMatricula).First())
            .Take(100)
            .Select(m => new AlumnoAptoDto(
                m.idAlumno,
                m.idAlumnoNavigation != null
                    ? $"{m.idAlumnoNavigation.primerNombre} {m.idAlumnoNavigation.segundoNombre} {m.idAlumnoNavigation.apellidoPaterno} {m.idAlumnoNavigation.apellidoMaterno}".Replace("  ", " ").Trim()
                    : m.idAlumno,
                m.idAlumnoNavigation != null ? (m.idAlumnoNavigation.email_institucional ?? string.Empty) : string.Empty,
                m.idAlumnoNavigation != null ? (m.idAlumnoNavigation.celular ?? string.Empty) : string.Empty,
                null,
                "Carrera Vinculada",
                m.idModalidad,
                m.idModalidadNavigation != null ? (m.idModalidadNavigation.modalidad ?? string.Empty) : "N/A",
                m.idPeriodo ?? string.Empty,
                "DISPONIBLE"
            ))
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IEnumerable<GraduadoHistoricoDto>> GetAlumnosGraduadosAsync(int? idCarrera, string? busqueda, CancellationToken cancellationToken = default)
    {
        var query = _context.alumnos_titulos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(at =>
                at.idAlumno.ToLower().Contains(term) ||
                (at.numero_acta != null && at.numero_acta.ToLower().Contains(term)) ||
                (at.titulo_tesis != null && at.titulo_tesis.ToLower().Contains(term))
            );
        }

        var result = await query
            .OrderByDescending(at => at.fecha)
            .Take(100)
            .Select(at => new GraduadoHistoricoDto(
                at.idAlumno,
                at.idAlumno,
                at.idTitulo,
                at.numero_acta ?? string.Empty,
                at.fecha_acta,
                at.nota_final,
                at.promedio_estudios,
                at.titulo_tesis ?? string.Empty
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
