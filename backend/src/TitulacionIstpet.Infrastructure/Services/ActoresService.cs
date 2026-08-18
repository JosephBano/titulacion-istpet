using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Actores;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class ActoresService : IActoresService
{
    private readonly SigafiDbContext _context;

    public ActoresService(SigafiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AlumnoResponseDto>> BuscarAlumnosAsync(string? busqueda, CancellationToken cancellationToken = default)
    {
        var query = _context.Alumnos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var pattern = $"%{busqueda.Trim()}%";
            query = query.Where(a =>
                EF.Functions.Like(a.IdAlumno, pattern) ||
                (a.PrimerNombre != null && EF.Functions.Like(a.PrimerNombre, pattern)) ||
                (a.SegundoNombre != null && EF.Functions.Like(a.SegundoNombre, pattern)) ||
                (a.ApellidoPaterno != null && EF.Functions.Like(a.ApellidoPaterno, pattern)) ||
                (a.ApellidoMaterno != null && EF.Functions.Like(a.ApellidoMaterno, pattern)) ||
                (a.EmailInstitucional != null && EF.Functions.Like(a.EmailInstitucional, pattern))
            );
        }

        return await query
            .OrderBy(a => a.ApellidoPaterno)
            .ThenBy(a => a.PrimerNombre)
            .Take(50)
            .Select(a => new AlumnoResponseDto(
                a.IdAlumno,
                $"{a.PrimerNombre} {a.SegundoNombre} {a.ApellidoPaterno} {a.ApellidoMaterno}".Replace("  ", " ").Trim(),
                a.PrimerNombre ?? string.Empty,
                a.SegundoNombre ?? string.Empty,
                a.ApellidoPaterno ?? string.Empty,
                a.ApellidoMaterno ?? string.Empty,
                a.EmailInstitucional ?? string.Empty,
                a.Email ?? string.Empty,
                a.Telefono ?? string.Empty,
                a.Celular ?? string.Empty,
                a.Direccion ?? string.Empty
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AlumnoResponseDto?> GetAlumnoPorCedulaAsync(string cedula, CancellationToken cancellationToken = default)
    {
        return await _context.Alumnos
            .AsNoTracking()
            .Where(a => a.IdAlumno == cedula)
            .Select(a => new AlumnoResponseDto(
                a.IdAlumno,
                $"{a.PrimerNombre} {a.SegundoNombre} {a.ApellidoPaterno} {a.ApellidoMaterno}".Replace("  ", " ").Trim(),
                a.PrimerNombre ?? string.Empty,
                a.SegundoNombre ?? string.Empty,
                a.ApellidoPaterno ?? string.Empty,
                a.ApellidoMaterno ?? string.Empty,
                a.EmailInstitucional ?? string.Empty,
                a.Email ?? string.Empty,
                a.Telefono ?? string.Empty,
                a.Celular ?? string.Empty,
                a.Direccion ?? string.Empty
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProfesorResponseDto>> GetDocentesEvaluadoresAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Profesores
            .AsNoTracking()
            .Where(p => p.Activo == true)
            .OrderBy(p => p.Apellidos)
            .Select(p => new ProfesorResponseDto(
                p.IdProfesor,
                $"{p.Nombres} {p.Apellidos}".Trim(),
                p.Nombres ?? string.Empty,
                p.Apellidos ?? string.Empty,
                "Docente Evaluador",
                "Ing.",
                p.Email ?? string.Empty,
                p.Celular ?? string.Empty,
                p.Activo == true
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProfesorResponseDto?> GetDocentePorCedulaAsync(string cedula, CancellationToken cancellationToken = default)
    {
        return await _context.Profesores
            .AsNoTracking()
            .Where(p => p.IdProfesor == cedula)
            .Select(p => new ProfesorResponseDto(
                p.IdProfesor,
                $"{p.Nombres} {p.Apellidos}".Trim(),
                p.Nombres ?? string.Empty,
                p.Apellidos ?? string.Empty,
                "Docente Evaluador",
                "Ing.",
                p.Email ?? string.Empty,
                p.Celular ?? string.Empty,
                p.Activo == true
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<MatriculaResponseDto>> GetMatriculasPorAlumnoAsync(string idAlumno, CancellationToken cancellationToken = default)
    {
        return await _context.Matriculas
            .AsNoTracking()
            .Include(m => m.IdModalidadNavigation)
            .Include(m => m.IdAlumnoNavigation)
            .Include(m => m.IdNivelNavigation)
            .Where(m => m.IdAlumno == idAlumno)
            .OrderByDescending(m => m.IdMatricula)
            .Select(m => new MatriculaResponseDto(
                m.IdMatricula,
                m.IdAlumno,
                m.IdAlumnoNavigation != null
                    ? $"{m.IdAlumnoNavigation.PrimerNombre} {m.IdAlumnoNavigation.SegundoNombre} {m.IdAlumnoNavigation.ApellidoPaterno} {m.IdAlumnoNavigation.ApellidoMaterno}".Replace("  ", " ").Trim()
                    : m.IdAlumno,
                m.IdNivelNavigation != null ? m.IdNivelNavigation.IdCarrera : 0,
                "Carrera Registrada",
                m.IdPeriodo ?? string.Empty,
                m.IdNivel,
                m.IdModalidad,
                m.IdModalidadNavigation != null ? (m.IdModalidadNavigation.Modalidad ?? string.Empty) : string.Empty
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AptitudTitulacionResponseDto> ValidarAptitudTitulacionAsync(string idAlumno, int idCarrera, CancellationToken cancellationToken = default)
    {
        var alumno = await _context.Alumnos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdAlumno == idAlumno, cancellationToken);

        if (alumno == null)
        {
            return new AptitudTitulacionResponseDto(idAlumno, "No Encontrado", "N/A", "N/A", false, false, "El estudiante no existe en el sistema.");
        }

        var carrera = await _context.Carreras
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCarrera == idCarrera, cancellationToken);

        var ultimaMatricula = await _context.Matriculas
            .AsNoTracking()
            .Where(m => m.IdAlumno == idAlumno && (m.Retirado == null || m.Retirado == false))
            .OrderByDescending(m => m.IdMatricula)
            .FirstOrDefaultAsync(cancellationToken);

        bool tieneMatricula = ultimaMatricula != null;
        bool esApto = tieneMatricula;

        string mensaje = esApto
            ? "El estudiante se encuentra registrado y apto para iniciar el proceso de postulación a titulación."
            : "El estudiante no registra una matrícula válida en el sistema para la carrera seleccionada.";

        return new AptitudTitulacionResponseDto(
            idAlumno,
            $"{alumno.PrimerNombre} {alumno.SegundoNombre} {alumno.ApellidoPaterno} {alumno.ApellidoMaterno}".Replace("  ", " ").Trim(),
            carrera?.Carrera ?? "N/A",
            ultimaMatricula?.IdPeriodo ?? "N/A",
            tieneMatricula,
            esApto,
            mensaje
        );
    }

    public async Task<IEnumerable<AlumnoAptoDto>> GetAlumnosAptosTitulacionAsync(int? idCarrera, int? idModalidad, string? busqueda, CancellationToken cancellationToken = default)
    {
        var query = _context.Matriculas
            .AsNoTracking()
            .Include(m => m.IdAlumnoNavigation)
            .Include(m => m.IdModalidadNavigation)
            .Where(m => !_context.AlumnosTitulos.Any(at =>
                at.IdAlumno == m.IdAlumno &&
                _context.Titulos.Any(t => t.IdTitulo == at.IdTitulo && _context.AlumnosCarreras.Any(ac => ac.IdAlumno == m.IdAlumno && ac.IdCarrera == t.IdCarrera))))
            .AsQueryable();

        if (idCarrera.HasValue && idCarrera.Value > 0)
        {
            query = query.Where(m => _context.Detallemallas.Any(dm => dm.IdAsignaturaNavigation != null && dm.IdMallaNavigation.IdCarrera == idCarrera.Value));
        }

        if (idModalidad.HasValue && idModalidad.Value > 0)
        {
            query = query.Where(m => m.IdModalidad == idModalidad.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var pattern = $"%{busqueda.Trim()}%";
            query = query.Where(m =>
                EF.Functions.Like(m.IdAlumno, pattern) ||
                (m.IdAlumnoNavigation != null && (
                    (m.IdAlumnoNavigation.PrimerNombre != null && EF.Functions.Like(m.IdAlumnoNavigation.PrimerNombre, pattern)) ||
                    (m.IdAlumnoNavigation.ApellidoPaterno != null && EF.Functions.Like(m.IdAlumnoNavigation.ApellidoPaterno, pattern))
                ))
            );
        }

        var result = await query
            .GroupBy(m => m.IdAlumno)
            .Select(g => g.OrderByDescending(m => m.IdMatricula).First())
            .Take(100)
            .Select(m => new AlumnoAptoDto(
                m.IdAlumno,
                m.IdAlumnoNavigation != null
                    ? $"{m.IdAlumnoNavigation.PrimerNombre} {m.IdAlumnoNavigation.SegundoNombre} {m.IdAlumnoNavigation.ApellidoPaterno} {m.IdAlumnoNavigation.ApellidoMaterno}".Replace("  ", " ").Trim()
                    : m.IdAlumno,
                m.IdAlumnoNavigation != null ? (m.IdAlumnoNavigation.EmailInstitucional ?? string.Empty) : string.Empty,
                m.IdAlumnoNavigation != null ? (m.IdAlumnoNavigation.Celular ?? string.Empty) : string.Empty,
                null,
                "Carrera Vinculada",
                m.IdModalidad,
                m.IdModalidadNavigation != null ? (m.IdModalidadNavigation.Modalidad ?? string.Empty) : "N/A",
                m.IdPeriodo ?? string.Empty,
                "DISPONIBLE"
            ))
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IEnumerable<GraduadoHistoricoDto>> GetAlumnosGraduadosAsync(int? idCarrera, string? busqueda, CancellationToken cancellationToken = default)
    {
        var query = _context.AlumnosTitulos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var pattern = $"%{busqueda.Trim()}%";
            query = query.Where(at =>
                EF.Functions.Like(at.IdAlumno, pattern) ||
                (at.NumeroActa != null && EF.Functions.Like(at.NumeroActa, pattern)) ||
                (at.TituloTesis != null && EF.Functions.Like(at.TituloTesis, pattern))
            );
        }

        var result = await query
            .OrderByDescending(at => at.Fecha)
            .Take(100)
            .Select(at => new GraduadoHistoricoDto(
                at.IdAlumno,
                at.IdAlumno,
                at.IdTitulo,
                at.NumeroActa ?? string.Empty,
                at.FechaActa,
                at.NotaFinal,
                at.PromedioEstudios,
                at.TituloTesis ?? string.Empty
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
