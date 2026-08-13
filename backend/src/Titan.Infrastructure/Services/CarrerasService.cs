using Microsoft.EntityFrameworkCore;
using Titan.Application.DTOs.Academico;
using Titan.Application.Interfaces;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Services;

public class CarrerasService : ICarrerasService
{
    private readonly TitanDbContext _context;

    // ID de la Escuela de Conducción a ignorar profesionalmente en Titulación ISTPET
    private const int ID_CARRERA_CONDUCCION = 6;

    public CarrerasService(TitanDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CarreraDto>> GetCarrerasTodasAsync(CancellationToken cancellationToken = default)
    {
        var consulta = from c in _context.carreras.AsNoTracking()
                       join mc in _context.modalidades_carreras.AsNoTracking() on c.idCarrera equals mc.idCarrera into mcGroup
                       from mc in mcGroup.DefaultIfEmpty()
                       join m in _context.modalidades.AsNoTracking() on mc.idModalidad equals m.idModalidad into mGroup
                       from m in mGroup.DefaultIfEmpty()
                       where (c.activa == true || c.activa == null) && c.idCarrera != ID_CARRERA_CONDUCCION && (c.esInstituto == 1 || (c.esInstituto == null && c.aliasCarrera != "CON"))
                       orderby c.ordenCarrera, c.Carrera, m.modalidad
                       select new
                       {
                           c.idCarrera,
                           NombreCarreraRaw = c.Carrera ?? string.Empty,
                           c.aliasCarrera,
                           c.codigo_cases,
                           c.activa,
                           IdModalidad = (int?)mc.idModalidad,
                           NombreModalidad = m != null ? m.modalidad : null
                       };

        var items = await consulta.ToListAsync(cancellationToken);

        var resultado = new List<CarreraDto>();
        foreach (var item in items)
        {
            string nombreMostrar = item.NombreCarreraRaw;
            if (!string.IsNullOrEmpty(item.NombreModalidad))
            {
                nombreMostrar = $"{item.NombreCarreraRaw} ({item.NombreModalidad})";
            }

            resultado.Add(new CarreraDto(
                item.idCarrera,
                nombreMostrar,
                item.aliasCarrera,
                item.codigo_cases,
                item.activa.HasValue && item.activa.Value,
                item.IdModalidad,
                item.NombreModalidad
            ));
        }

        return resultado;
    }

    public async Task<IEnumerable<EstudianteCarreraDto>> GetCarrerasPorEstudianteAsync(string idAlumno, CancellationToken cancellationToken = default)
    {
        var carrerasAlumno = await _context.alumnos_carreras
            .AsNoTracking()
            .Where(ac => ac.idAlumno == idAlumno && ac.idCarrera != ID_CARRERA_CONDUCCION)
            .ToListAsync(cancellationToken);

        var idsCarreras = carrerasAlumno.Select(ac => ac.idCarrera).Distinct().ToList();

        if (idsCarreras.Count == 0)
        {
            var matriculasAlumno = await _context.matriculas
                .AsNoTracking()
                .Where(m => m.idAlumno == idAlumno && (m.retirado == null || m.retirado == false))
                .ToListAsync(cancellationToken);

            if (matriculasAlumno.Count == 0) return Enumerable.Empty<EstudianteCarreraDto>();
        }

        // Obtener la última matrícula del estudiante para conocer su modalidad real de cursado
        var ultimaMatricula = await _context.matriculas
            .AsNoTracking()
            .Include(m => m.idModalidadNavigation)
            .Where(m => m.idAlumno == idAlumno && (m.retirado == null || m.retirado == false))
            .OrderByDescending(m => m.idMatricula)
            .FirstOrDefaultAsync(cancellationToken);

        var carrerasInfo = await _context.carreras
            .AsNoTracking()
            .Where(c => idsCarreras.Contains(c.idCarrera) && c.idCarrera != ID_CARRERA_CONDUCCION)
            .ToListAsync(cancellationToken);

        var titulosAlumno = await (from at in _context.alumnos_titulos.AsNoTracking()
                                   join t in _context.titulos.AsNoTracking() on at.idTitulo equals t.idTitulo
                                   where at.idAlumno == idAlumno && t.idCarrera.HasValue && t.idCarrera.Value != ID_CARRERA_CONDUCCION
                                   select new { idCarrera = t.idCarrera.Value, codigoSistema = at.codigo_sistema })
                                  .ToListAsync(cancellationToken);

        var matriculasVigentes = await _context.matriculas
            .AsNoTracking()
            .Where(m => m.idAlumno == idAlumno && (m.retirado == null || m.retirado == false))
            .AnyAsync(cancellationToken);

        var resultado = new List<EstudianteCarreraDto>();
        foreach (var c in carrerasInfo)
        {
            var tituloInfo = titulosAlumno.FirstOrDefault(t => t.idCarrera == c.idCarrera);
            bool estaTitulado = tituloInfo != null;
            string? codigoSistema = tituloInfo != null ? tituloInfo.codigoSistema.ToString() : null;

            int? idModalidad = ultimaMatricula?.idModalidad;
            string? nombreModalidad = ultimaMatricula?.idModalidadNavigation?.modalidad;

            string nombreCarreraMostrar = c.Carrera ?? string.Empty;
            if (!string.IsNullOrEmpty(nombreModalidad))
            {
                nombreCarreraMostrar = $"{c.Carrera} ({nombreModalidad})";
            }

            resultado.Add(new EstudianteCarreraDto(
                c.idCarrera,
                nombreCarreraMostrar,
                c.aliasCarrera,
                estaTitulado,
                codigoSistema,
                matriculasVigentes && !estaTitulado,
                idModalidad,
                nombreModalidad
            ));
        }

        return resultado;
    }

    public async Task<IEnumerable<ProfesorCarreraDto>> GetCarrerasPorProfesorAsync(string idProfesor, CancellationToken cancellationToken = default)
    {
        var asignaciones = await _context.profesores_carreras_periodos
            .AsNoTracking()
            .Include(pcp => pcp.idCarreraNavigation)
            .Include(pcp => pcp.idPeriodoNavigation)
            .Where(pcp => pcp.idProfesor == idProfesor && (pcp.esActivo == 1 || pcp.esActivo == null) && (pcp.idCarrera == null || pcp.idCarrera != ID_CARRERA_CONDUCCION))
            .ToListAsync(cancellationToken);

        bool sonTodas = asignaciones.Any(pcp => pcp.sonTodas == 1);

        if (sonTodas)
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
        var gruposPorCarrera = asignaciones.Where(pcp => pcp.idCarreraNavigation != null && pcp.idCarrera != ID_CARRERA_CONDUCCION)
                                            .GroupBy(pcp => pcp.idCarrera);

        foreach (var grupo in gruposPorCarrera)
        {
            var primerItem = grupo.First();
            var carrera = primerItem.idCarreraNavigation!;
            string? periodo = primerItem.idPeriodoNavigation?.idPeriodo;

            resultado.Add(new ProfesorCarreraDto(
                carrera.idCarrera,
                carrera.Carrera ?? string.Empty,
                carrera.aliasCarrera,
                false,
                periodo
            ));
        }

        return resultado;
    }

    public async Task<UsuarioCarrerasResponseDto?> GetCarrerasUsuarioAutenticadoAsync(string idSigafi, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.idSigafi == idSigafi, cancellationToken);

        if (usuario == null) return null;

        var carrerasEstudiante = await GetCarrerasPorEstudianteAsync(idSigafi, cancellationToken);
        var carrerasDocente = await GetCarrerasPorProfesorAsync(idSigafi, cancellationToken);

        string tipoUsuario = "ESTUDIANTE";
        if (carrerasEstudiante.Any() && carrerasDocente.Any()) tipoUsuario = "AMBOS";
        else if (carrerasDocente.Any()) tipoUsuario = "DOCENTE";
        else if (string.Equals(usuario.tablaSigafi, "profesor", StringComparison.OrdinalIgnoreCase)) tipoUsuario = "DOCENTE";

        return new UsuarioCarrerasResponseDto(
            idSigafi,
            usuario.nombre ?? string.Empty,
            tipoUsuario,
            carrerasEstudiante,
            carrerasDocente
        );
    }
}
