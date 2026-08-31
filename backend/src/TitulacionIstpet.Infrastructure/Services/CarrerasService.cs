using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Academico;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class CarrerasService : ICarrerasService
{
    private readonly SigafiDbContext _context;

    public CarrerasService(SigafiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CarreraDto>> GetCarrerasTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Carreras
            .AsNoTracking()
            .Where(c => (c.EsInstituto == true) && (c.Activa == true || c.Activa == null))
            .OrderBy(c => c.Carrera)
            .Select(c => new CarreraDto(
                c.IdCarrera,
                c.Carrera ?? string.Empty,
                c.AliasCarrera,
                c.CodigoCases,
                c.Activa ?? false,
                null,
                null
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EstudianteCarreraDto>> GetCarrerasPorEstudianteAsync(string idAlumno, CancellationToken cancellationToken = default)
    {
        // 1. Carreras en las que el alumno está registrado
        var carrerasAlumno = await _context.AlumnosCarreras
            .AsNoTracking()
            .Where(ac => ac.IdAlumno == idAlumno)
            .Join(_context.Carreras, ac => ac.IdCarrera, c => c.IdCarrera, (ac, c) => new { ac, c })
            .Where(x => (x.c.EsInstituto == true) && (x.c.Activa == true || x.c.Activa == null))
            .ToListAsync(cancellationToken);

        // 2. Títulos obtenidos por el alumno
        var titulosAlumno = await _context.AlumnosTitulos
            .AsNoTracking()
            .Where(at => at.IdAlumno == idAlumno)
            .Join(_context.Titulos, at => at.IdTitulo, t => t.IdTitulo, (at, t) => new { at, t })
            .ToListAsync(cancellationToken);

        // 3. Matrícula activa del alumno para determinar modalidad de estudio
        var matriculaVigente = await _context.Matriculas
            .AsNoTracking()
            .Include(m => m.IdModalidadNavigation)
            .Where(m => m.IdAlumno == idAlumno && (m.Retirado == null || m.Retirado == false))
            .OrderByDescending(m => m.IdMatricula)
            .FirstOrDefaultAsync(cancellationToken);

        var resultado = new List<EstudianteCarreraDto>();

        foreach (var item in carrerasAlumno)
        {
            var carrera = item.c;
            var titulo = titulosAlumno.FirstOrDefault(t => t.t.IdCarrera == carrera.IdCarrera);
            bool estaTitulado = titulo != null;
            string? codigoSistema = titulo?.at.CodigoSistema?.ToString();

            resultado.Add(new EstudianteCarreraDto(
                carrera.IdCarrera,
                carrera.Carrera ?? string.Empty,
                carrera.AliasCarrera,
                estaTitulado,
                codigoSistema,
                matriculaVigente != null,
                matriculaVigente?.IdModalidad,
                matriculaVigente?.IdModalidadNavigation?.Modalidad
            ));
        }

        return resultado;
    }

    public async Task<IEnumerable<ProfesorCarreraDto>> GetCarrerasPorProfesorAsync(string idProfesor, CancellationToken cancellationToken = default)
    {
        var asignaciones = await _context.ProfesoresCarrerasPeriodos
            .AsNoTracking()
            .Include(pcp => pcp.IdCarreraNavigation)
            .Include(pcp => pcp.IdPeriodoNavigation)
            .Where(pcp => pcp.IdProfesor == idProfesor && pcp.EsActivo == true)
            .ToListAsync(cancellationToken);

        if (asignaciones.Any(pcp => pcp.SonTodas == true))
        {
            var todasCarreras = await GetCarrerasTodasAsync(cancellationToken);
            return todasCarreras.Select(c => new ProfesorCarreraDto(
                c.IdCarrera,
                c.NombreCarrera,
                c.AliasCarrera,
                true,
                "Todas",
                c.IdModalidad,
                c.NombreModalidad
            ));
        }

        var resultado = new List<ProfesorCarreraDto>();
        var gruposPorCarrera = asignaciones
            .Where(pcp => pcp.IdCarreraNavigation != null && (pcp.IdCarreraNavigation.EsInstituto == true))
            .GroupBy(pcp => pcp.IdCarrera);

        foreach (var grupo in gruposPorCarrera)
        {
            var primerItem = grupo.First();
            var carrera = primerItem.IdCarreraNavigation!;
            string? periodo = primerItem.IdPeriodoNavigation?.IdPeriodo;

            resultado.Add(new ProfesorCarreraDto(
                carrera.IdCarrera,
                carrera.Carrera ?? string.Empty,
                carrera.AliasCarrera,
                false,
                periodo
            ));
        }

        return resultado;
    }

    public async Task<UsuarioCarrerasResponseDto?> GetCarrerasUsuarioAutenticadoAsync(string idSigafi, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdSigafi == idSigafi, cancellationToken);

        if (usuario == null)
        {
            return null;
        }

        var carrerasEstudiante = await GetCarrerasPorEstudianteAsync(idSigafi, cancellationToken);
        var carrerasDocente = await GetCarrerasPorProfesorAsync(idSigafi, cancellationToken);

        string tipoUsuario = "ESTUDIANTE";
        if (carrerasEstudiante.Any() && carrerasDocente.Any())
        {
            tipoUsuario = "AMBOS";
        }
        else if (carrerasDocente.Any())
        {
            tipoUsuario = "DOCENTE";
        }
        else if (string.Equals(usuario.TablaSigafi, "profesor", StringComparison.OrdinalIgnoreCase))
        {
            tipoUsuario = "DOCENTE";
        }

        return new UsuarioCarrerasResponseDto(
            idSigafi,
            usuario.Nombre ?? string.Empty,
            tipoUsuario,
            carrerasEstudiante,
            carrerasDocente
        );
    }
}
