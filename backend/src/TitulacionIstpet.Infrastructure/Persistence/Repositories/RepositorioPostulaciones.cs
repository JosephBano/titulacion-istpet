using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public sealed class RepositorioPostulaciones(SigafiDbContext context) : IRepositorioPostulaciones
{
    private readonly SigafiDbContext _context = context;

    public async Task<ElegibilidadPostulacionDto> ObtenerElegibilidadEstudianteAsync(
        string idAlumno, CancellationToken ct = default)
    {
        var alumno = await _context.Alumnos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdAlumno == idAlumno, ct);

        if (alumno == null)
        {
            return new ElegibilidadPostulacionDto(
                EsElegible: false,
                Mensaje: "El estudiante no se encuentra registrado en el sistema institucional.",
                IdMatricula: null,
                IdAlumno: idAlumno,
                NombreCompleto: null,
                IdCarrera: null,
                NombreCarrera: null,
                IdCohorte: null,
                DetalleCohorte: null,
                TienePostulacionActiva: false,
                IdPostulacionActiva: null,
                EstadoPostulacionActiva: null,
                ModalidadesOfertadas: Array.Empty<ModalidadOfertadaDto>()
            );
        }

        var nombreCompleto = $"{alumno.PrimerNombre} {alumno.SegundoNombre} {alumno.ApellidoPaterno} {alumno.ApellidoMaterno}".Replace("  ", " ").Trim();

        // Buscar matrícula vigente con su nivel y carrera
        var matricula = await _context.Matriculas
            .AsNoTracking()
            .Include(m => m.IdNivelNavigation)
                .ThenInclude(n => n.IdCarreraNavigation)
            .Include(m => m.IdModalidadNavigation)
            .Where(m => m.IdAlumno == idAlumno && (m.Retirado == false || m.Retirado == null))
            .OrderByDescending(m => m.FechaMatricula ?? DateTime.MinValue)
            .FirstOrDefaultAsync(ct);

        if (matricula == null)
        {
            return new ElegibilidadPostulacionDto(
                EsElegible: false,
                Mensaje: "No registra una matrícula activa en la institución para el periodo vigente.",
                IdMatricula: null,
                IdAlumno: idAlumno,
                NombreCompleto: nombreCompleto,
                IdCarrera: null,
                NombreCarrera: null,
                IdCohorte: null,
                DetalleCohorte: null,
                TienePostulacionActiva: false,
                IdPostulacionActiva: null,
                EstadoPostulacionActiva: null,
                ModalidadesOfertadas: Array.Empty<ModalidadOfertadaDto>()
            );
        }

        var idCarrera = matricula.IdNivelNavigation?.IdCarrera;
        var nombreCarrera = matricula.IdNivelNavigation?.IdCarreraNavigation?.Carrera ?? "Carrera no identificada";

        // Regla institucional: Estudiantes de 1ro, 2do y 3er nivel/semestre no pueden postularse
        var nivelObj = matricula.IdNivelNavigation;
        bool esNivelInicial = false;
        if (nivelObj != null)
        {
            if (nivelObj.Jerarquia.HasValue && nivelObj.Jerarquia.Value <= 3 && nivelObj.Jerarquia.Value > 0)
            {
                esNivelInicial = true;
            }
            else if (nivelObj.Orden.HasValue && nivelObj.Orden.Value <= 3 && nivelObj.Orden.Value > 0)
            {
                esNivelInicial = true;
            }
            else if (!string.IsNullOrWhiteSpace(nivelObj.Nivel))
            {
                var nUpper = nivelObj.Nivel.ToUpperInvariant();
                if (nUpper.Contains("PRIMER") || nUpper.Contains("SEGUND") || nUpper.Contains("TERCER") ||
                    nUpper.StartsWith('1') || nUpper.StartsWith('2') || nUpper.StartsWith('3'))
                {
                    esNivelInicial = true;
                }
            }
        }

        if (esNivelInicial)
        {
            return new ElegibilidadPostulacionDto(
                EsElegible: false,
                Mensaje: "Los estudiantes de 1ro, 2do y 3er nivel no están habilitados para participar en el proceso de titulación. El proceso está disponible a partir de 4to nivel o egresados.",
                IdMatricula: matricula.IdMatricula,
                IdAlumno: idAlumno,
                NombreCompleto: nombreCompleto,
                IdCarrera: idCarrera,
                NombreCarrera: nombreCarrera,
                IdCohorte: null,
                DetalleCohorte: null,
                TienePostulacionActiva: false,
                IdPostulacionActiva: null,
                EstadoPostulacionActiva: null,
                ModalidadesOfertadas: Array.Empty<ModalidadOfertadaDto>()
            );
        }

        // Buscar cohorte activa para la carrera/modalidad
        var cohorteCarrera = await _context.TitulCohortesCarreras
            .AsNoTracking()
            .Include(cc => cc.IdCohorteNavigation)
            .Include(cc => cc.IdModalidadCarreraNavigation)
            .Where(cc => cc.EsActivo == true &&
                         cc.IdCohorteNavigation.EsActivo == true &&
                         cc.IdModalidadCarreraNavigation.IdCarrera == idCarrera &&
                         cc.IdModalidadCarreraNavigation.IdModalidad == matricula.IdModalidad)
            .OrderByDescending(cc => cc.IdCohorte)
            .FirstOrDefaultAsync(ct);

        // Si no encontró por modalidad exacta, buscar cohorte activa de la carrera
        if (cohorteCarrera == null && idCarrera.HasValue)
        {
            cohorteCarrera = await _context.TitulCohortesCarreras
                .AsNoTracking()
                .Include(cc => cc.IdCohorteNavigation)
                .Include(cc => cc.IdModalidadCarreraNavigation)
                .Where(cc => cc.EsActivo == true &&
                             cc.IdCohorteNavigation.EsActivo == true &&
                             cc.IdModalidadCarreraNavigation.IdCarrera == idCarrera.Value)
                .OrderByDescending(cc => cc.IdCohorte)
                .FirstOrDefaultAsync(ct);
        }

        // Verificar si ya tiene postulación activa
        var postulacionActiva = await _context.TitulPostulacionAlumnos
            .AsNoTracking()
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Where(p => p.IdMatriculaNavigation.IdAlumno == idAlumno && p.EsActivo == true)
            .OrderByDescending(p => p.IdPostulacionAlumnos)
            .FirstOrDefaultAsync(ct);

        bool tienePostulacion = postulacionActiva != null;
        int? idPostulacion = postulacionActiva?.IdPostulacionAlumnos;
        string? estadoPostulacion = postulacionActiva?.IdPostulacionEstadoNavigation?.Nombre;

        var modalidadesOfertadas = new List<ModalidadOfertadaDto>();
        if (cohorteCarrera != null)
        {
            modalidadesOfertadas = (await ListarModalidadesOfertadasPorCohorteCarreraAsync(cohorteCarrera.IdCohorteCarrera, ct)).ToList();
        }

        bool esElegible = !tienePostulacion && cohorteCarrera != null && modalidadesOfertadas.Count > 0;
        string mensaje = esElegible
            ? "Estudiante habilitado para postularse al proceso de titulación."
            : (tienePostulacion
                ? $"Ya registra una postulación activa en estado: '{estadoPostulacion}'."
                : (cohorteCarrera == null
                    ? "No existe una cohorte de titulación activa para su carrera en el periodo actual."
                    : "No existen modalidades de titulación configuradas para su carrera."));

        return new ElegibilidadPostulacionDto(
            EsElegible: esElegible,
            Mensaje: mensaje,
            IdMatricula: matricula.IdMatricula,
            IdAlumno: idAlumno,
            NombreCompleto: nombreCompleto,
            IdCarrera: idCarrera,
            NombreCarrera: nombreCarrera,
            IdCohorte: cohorteCarrera?.IdCohorte,
            DetalleCohorte: cohorteCarrera?.IdCohorteNavigation?.Detelle,
            TienePostulacionActiva: tienePostulacion,
            IdPostulacionActiva: idPostulacion,
            EstadoPostulacionActiva: estadoPostulacion,
            ModalidadesOfertadas: modalidadesOfertadas
        );
    }

    public async Task<IReadOnlyList<ModalidadOfertadaDto>> ListarModalidadesOfertadasPorCohorteCarreraAsync(
        int idCohorteCarrera, CancellationToken ct = default)
    {
        var modalidades = await _context.TitulModalidadesTitulacionCarreras
            .AsNoTracking()
            .Include(mtc => mtc.IdModalidadTitulacionNavigation)
                .ThenInclude(m => m.TitulRequisitoModalidad.Where(rm => rm.EsActivo == true))
                    .ThenInclude(rm => rm.IdRequisitosNavigation)
            .Where(mtc => mtc.IdCohorteCarrera == idCohorteCarrera && mtc.EsActivo == true)
            .ToListAsync(ct);

        return modalidades.Select(m => new ModalidadOfertadaDto(
            IdModalidadTitulacionCarrera: m.IdModalidadTitulacionCarrera,
            IdModalidadTitulacion: m.IdModalidadTitulacion,
            ModalidadTitulacion: m.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? string.Empty,
            EsComplexivo: m.IdModalidadTitulacionNavigation?.EsComplexivo,
            EsArticuloCientifico: m.IdModalidadTitulacionNavigation?.EsArticuloCientifico,
            GeneraTesis: m.IdModalidadTitulacionNavigation?.GeneraTesis,
            Requisitos: m.IdModalidadTitulacionNavigation?.TitulRequisitoModalidad
                .Where(rm => rm.EsActivo == true && rm.IdRequisitosNavigation != null && rm.IdRequisitosNavigation.EsActivo == true)
                .Select(rm => new RequisitoModalidadOfertadaDto(
                    IdRequisitoModalidad: rm.IdRequisitoModalidad,
                    IdRequisitos: rm.IdRequisitos,
                    NombreRequisito: rm.IdRequisitosNavigation?.Requisito ?? string.Empty,
                    EsAdjunto: rm.IdRequisitosNavigation?.EsAdjunto == true,
                    EsBool: rm.IdRequisitosNavigation?.EsBool == true,
                    SubeAlumno: rm.IdRequisitosNavigation?.SubeAlumno == true,
                    SubeColaborador: rm.IdRequisitosNavigation?.SubeColaborador == true,
                    EsRequisitoFinal: rm.EsRequistoFinal == true
                ))
                .ToList() ?? new List<RequisitoModalidadOfertadaDto>()
        )).ToList();
    }

    public async Task<PostulacionDetalleDto?> ObtenerMiPostulacionActivaAsync(
        string idAlumno, CancellationToken ct = default)
    {
        var postulacionId = await _context.TitulPostulacionAlumnos
            .Where(p => p.IdMatriculaNavigation.IdAlumno == idAlumno && p.EsActivo == true)
            .OrderByDescending(p => p.IdPostulacionAlumnos)
            .Select(p => p.IdPostulacionAlumnos)
            .FirstOrDefaultAsync(ct);

        if (postulacionId == 0)
        {
            return null;
        }

        await SincronizarRequisitosModalidadAsync(postulacionId, ct);

        var postulacion = await _context.TitulPostulacionAlumnos
            .AsNoTracking()
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdNivelNavigation)
                    .ThenInclude(n => n.IdCarreraNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdCohorteCarreraNavigation)
                    .ThenInclude(cc => cc.IdCohorteNavigation)
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(parm => parm.IdRequisitoModalidadNavigation)
                    .ThenInclude(rm => rm.IdRequisitosNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(parm => parm.IdAdjuntosImagenesNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(parm => parm.TitulResponsableEvidencia)
                    .ThenInclude(tre => tre.IdResponsableEvidenciasNavigation)
                        .ThenInclude(trr => trr.IdProfesorNavigation)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == postulacionId, ct);

        if (postulacion == null)
        {
            return null;
        }

        return MapearDetalle(postulacion);
    }

    public async Task<PostulacionDetalleDto?> ObtenerPorIdAsync(
        int idPostulacionAlumnos, CancellationToken ct = default)
    {
        await SincronizarRequisitosModalidadAsync(idPostulacionAlumnos, ct);

        var postulacion = await _context.TitulPostulacionAlumnos
            .AsNoTracking()
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdNivelNavigation)
                    .ThenInclude(n => n.IdCarreraNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdCohorteCarreraNavigation)
                    .ThenInclude(cc => cc.IdCohorteNavigation)
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(parm => parm.IdRequisitoModalidadNavigation)
                    .ThenInclude(rm => rm.IdRequisitosNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(parm => parm.IdAdjuntosImagenesNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(parm => parm.TitulResponsableEvidencia)
                    .ThenInclude(tre => tre.IdResponsableEvidenciasNavigation)
                        .ThenInclude(trr => trr.IdProfesorNavigation)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == idPostulacionAlumnos, ct);

        if (postulacion == null)
        {
            return null;
        }

        return MapearDetalle(postulacion);
    }

    private async Task SincronizarRequisitosModalidadAsync(int idPostulacionAlumnos, CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == idPostulacionAlumnos, ct);

        if (postulacion == null)
        {
            return;
        }

        int idModalidad = postulacion.IdModalidadTitulacionCarreraNavigation?.IdModalidadTitulacion ?? 0;
        if (idModalidad == 0)
        {
            idModalidad = await _context.TitulModalidadesTitulacionCarreras
                .Where(m => m.IdModalidadTitulacionCarrera == postulacion.IdModalidadTitulacionCarrera)
                .Select(m => m.IdModalidadTitulacion)
                .FirstOrDefaultAsync(ct);
        }

        if (idModalidad == 0)
        {
            return;
        }

        // Requisitos activos de la modalidad
        var requisitosModalidadActivos = await _context.TitulRequisitoModalidad
            .Where(rm => rm.IdModalidadTitulacion == idModalidad && rm.EsActivo == true)
            .Select(rm => rm.IdRequisitoModalidad)
            .ToListAsync(ct);

        if (requisitosModalidadActivos.Count == 0)
        {
            return;
        }

        // Requisitos ya existentes en el expediente de la postulación
        var requisitosExistentes = await _context.TitulPostulacionAlumnosRequisitosModalidad
            .Where(r => r.IdPostulacionAlumnos == idPostulacionAlumnos)
            .Select(r => r.IdRequisitoModalidad)
            .ToListAsync(ct);

        var faltantes = requisitosModalidadActivos.Except(requisitosExistentes).ToList();
        if (faltantes.Count > 0)
        {
            foreach (var idReqMod in faltantes)
            {
                _context.TitulPostulacionAlumnosRequisitosModalidad.Add(new TitulPostulacionAlumnosRequisitosModalidad
                {
                    IdPostulacionAlumnos = idPostulacionAlumnos,
                    IdRequisitoModalidad = idReqMod,
                    IdAdjuntosImagenes = null,
                    ValorBool = false
                });
            }
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<PaginaPostulacionesDto> ListarPostulacionesAsync(
        int? idCohorte,
        int? idCarrera,
        int? idModalidad,
        int? idEstado,
        string? busqueda,
        int pagina,
        int tamanoPagina,
        CancellationToken ct = default)
    {
        var query = _context.TitulPostulacionAlumnos
            .AsNoTracking()
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdNivelNavigation)
                    .ThenInclude(n => n.IdCarreraNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdCohorteCarreraNavigation)
                    .ThenInclude(cc => cc.IdCohorteNavigation)
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
            .AsQueryable();

        if (idCohorte.HasValue && idCohorte.Value > 0)
        {
            query = query.Where(p => p.IdModalidadTitulacionCarreraNavigation.IdCohorteCarreraNavigation.IdCohorte == idCohorte.Value);
        }

        if (idCarrera.HasValue && idCarrera.Value > 0)
        {
            query = query.Where(p => p.IdMatriculaNavigation.IdNivelNavigation.IdCarrera == idCarrera.Value);
        }

        if (idModalidad.HasValue && idModalidad.Value > 0)
        {
            query = query.Where(p => p.IdModalidadTitulacionCarreraNavigation.IdModalidadTitulacion == idModalidad.Value);
        }

        if (idEstado.HasValue && idEstado.Value > 0)
        {
            query = query.Where(p => p.IdPostulacionEstado == idEstado.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim();
            query = query.Where(p =>
                (p.IdMatriculaNavigation.IdAlumno != null && p.IdMatriculaNavigation.IdAlumno.Contains(term)) ||
                (p.IdMatriculaNavigation.IdAlumnoNavigation.PrimerNombre != null && p.IdMatriculaNavigation.IdAlumnoNavigation.PrimerNombre.Contains(term)) ||
                (p.IdMatriculaNavigation.IdAlumnoNavigation.ApellidoPaterno != null && p.IdMatriculaNavigation.IdAlumnoNavigation.ApellidoPaterno.Contains(term)) ||
                (p.IdMatriculaNavigation.IdAlumnoNavigation.ApellidoMaterno != null && p.IdMatriculaNavigation.IdAlumnoNavigation.ApellidoMaterno.Contains(term)));
        }

        int total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.IdPostulacionAlumnos)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(p => new PostulacionResumenDto(
                p.IdPostulacionAlumnos,
                p.IdMatricula,
                p.IdMatriculaNavigation.IdAlumno ?? string.Empty,
                $"{p.IdMatriculaNavigation.IdAlumnoNavigation.PrimerNombre} {p.IdMatriculaNavigation.IdAlumnoNavigation.ApellidoPaterno}".Trim(),
                p.IdMatriculaNavigation.IdAlumno ?? string.Empty,
                p.IdMatriculaNavigation.IdNivelNavigation.IdCarrera,
                p.IdMatriculaNavigation.IdNivelNavigation.IdCarreraNavigation.Carrera ?? string.Empty,
                p.IdModalidadTitulacionCarreraNavigation.IdCohorteCarreraNavigation.IdCohorte,
                p.IdModalidadTitulacionCarreraNavigation.IdCohorteCarreraNavigation.IdCohorteNavigation.Detelle ?? string.Empty,
                p.IdModalidadTitulacionCarrera,
                p.IdModalidadTitulacionCarreraNavigation.IdModalidadTitulacionNavigation.ModalidadTitulacion ?? string.Empty,
                p.IdPostulacionEstado,
                p.IdPostulacionEstadoNavigation.Nombre ?? string.Empty,
                p.EsActivo,
                p.EsCambioModalidad,
                p.TitulPostulacionAlumnosRequisitosModalidad.Count,
                p.TitulPostulacionAlumnosRequisitosModalidad.Count(r => r.IdAdjuntosImagenes != null || r.ValorBool == true)
            ))
            .ToListAsync(ct);

        return new PaginaPostulacionesDto(items, pagina, tamanoPagina, total);
    }

    public async Task<IReadOnlyList<EstadoPostulacionDto>> ListarEstadosAsync(CancellationToken ct = default)
    {
        var estados = await _context.TitulPostulacionEstados
            .AsNoTracking()
            .Where(e => e.EsActivo == true)
            .OrderBy(e => e.Orden)
            .ToListAsync(ct);

        if (estados.Count == 0)
        {
            var estadosBase = new List<TitulPostulacionEstados>
            {
                new() { IdPostulacionEstado = 1, Nombre = "EN REVISIÓN", Orden = 1, EsFinal = false, EsActivo = true },
                new() { IdPostulacionEstado = 2, Nombre = "APROBADO", Orden = 2, EsFinal = true, EsActivo = true },
                new() { IdPostulacionEstado = 3, Nombre = "OBSERVADO", Orden = 3, EsFinal = false, EsActivo = true },
                new() { IdPostulacionEstado = 4, Nombre = "RECHAZADO", Orden = 4, EsFinal = true, EsActivo = true }
            };

            await _context.TitulPostulacionEstados.AddRangeAsync(estadosBase, ct);
            await _context.SaveChangesAsync(ct);
            estados = estadosBase;
        }

        return estados.Select(e => new EstadoPostulacionDto(
            e.IdPostulacionEstado,
            e.Nombre ?? string.Empty,
            e.Orden,
            e.EsFinal,
            e.EsActivo
        )).ToList();
    }

    public async Task<int> CrearPostulacionAsync(
        int idMatricula,
        int idModalidadTitulacionCarrera,
        IReadOnlyList<RequisitoPostulacionInputDto>? requisitos,
        CancellationToken ct = default)
    {
        var matricula = await _context.Matriculas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMatricula == idMatricula, ct)
            ?? throw new NoEncontradoException("Matrícula", idMatricula);

        // Verificar que no tenga otra postulación activa
        bool tieneActiva = await _context.TitulPostulacionAlumnos
            .AnyAsync(p => p.IdMatriculaNavigation.IdAlumno == matricula.IdAlumno && p.EsActivo == true, ct);

        if (tieneActiva)
        {
            throw new InvalidOperationException("El estudiante ya posee una postulación activa en el sistema.");
        }

        // Obtener estado inicial (menor orden) o auto-sembrar si está vacío
        var estadoInicial = await _context.TitulPostulacionEstados
            .Where(e => e.EsActivo == true)
            .OrderBy(e => e.Orden)
            .FirstOrDefaultAsync(ct);

        if (estadoInicial == null)
        {
            var estadosBase = new List<TitulPostulacionEstados>
            {
                new() { IdPostulacionEstado = 1, Nombre = "EN REVISIÓN", Orden = 1, EsFinal = false, EsActivo = true },
                new() { IdPostulacionEstado = 2, Nombre = "APROBADO", Orden = 2, EsFinal = true, EsActivo = true },
                new() { IdPostulacionEstado = 3, Nombre = "OBSERVADO", Orden = 3, EsFinal = false, EsActivo = true },
                new() { IdPostulacionEstado = 4, Nombre = "RECHAZADO", Orden = 4, EsFinal = true, EsActivo = true }
            };

            await _context.TitulPostulacionEstados.AddRangeAsync(estadosBase, ct);
            await _context.SaveChangesAsync(ct);
            estadoInicial = estadosBase[0];
        }

        int idEstado = estadoInicial.IdPostulacionEstado;

        var postulacion = new TitulPostulacionAlumnos
        {
            IdMatricula = idMatricula,
            IdModalidadTitulacionCarrera = idModalidadTitulacionCarrera,
            IdPostulacionEstado = idEstado,
            EsActivo = true,
            EsCambioModalidad = false
        };

        _context.TitulPostulacionAlumnos.Add(postulacion);
        await _context.SaveChangesAsync(ct);

        if (requisitos != null && requisitos.Count > 0)
        {
            foreach (var req in requisitos)
            {
                int? idAdjunto = (req.IdAdjuntosImagenes.HasValue && req.IdAdjuntosImagenes.Value > 0)
                    ? req.IdAdjuntosImagenes.Value
                    : null;

                var reqEntity = new TitulPostulacionAlumnosRequisitosModalidad
                {
                    IdPostulacionAlumnos = postulacion.IdPostulacionAlumnos,
                    IdRequisitoModalidad = req.IdRequisitoModalidad,
                    IdAdjuntosImagenes = idAdjunto,
                    ValorBool = req.ValorBool
                };
                _context.TitulPostulacionAlumnosRequisitosModalidad.Add(reqEntity);
            }
            await _context.SaveChangesAsync(ct);
        }

        // Sincronizar todos los requisitos activos que la modalidad tenga configurados
        await SincronizarRequisitosModalidadAsync(postulacion.IdPostulacionAlumnos, ct);

        return postulacion.IdPostulacionAlumnos;
    }

    public async Task ActualizarRequisitosAsync(
        int idPostulacionAlumnos,
        IReadOnlyList<RequisitoPostulacionInputDto> requisitos,
        CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == idPostulacionAlumnos, ct)
            ?? throw new NoEncontradoException("Postulación", idPostulacionAlumnos);

        foreach (var reqInput in requisitos)
        {
            int? idAdjunto = (reqInput.IdAdjuntosImagenes.HasValue && reqInput.IdAdjuntosImagenes.Value > 0)
                ? reqInput.IdAdjuntosImagenes.Value
                : null;

            var existente = postulacion.TitulPostulacionAlumnosRequisitosModalidad
                .FirstOrDefault(r => r.IdRequisitoModalidad == reqInput.IdRequisitoModalidad);

            if (existente != null)
            {
                if (idAdjunto.HasValue)
                {
                    existente.IdAdjuntosImagenes = idAdjunto.Value;
                }
                if (reqInput.ValorBool.HasValue)
                {
                    existente.ValorBool = reqInput.ValorBool.Value;
                }
            }
            else
            {
                var nuevo = new TitulPostulacionAlumnosRequisitosModalidad
                {
                    IdPostulacionAlumnos = idPostulacionAlumnos,
                    IdRequisitoModalidad = reqInput.IdRequisitoModalidad,
                    IdAdjuntosImagenes = idAdjunto,
                    ValorBool = reqInput.ValorBool
                };
                _context.TitulPostulacionAlumnosRequisitosModalidad.Add(nuevo);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task CambiarEstadoAsync(
        int idPostulacionAlumnos,
        int idNuevoEstado,
        CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.IdRequisitoModalidadNavigation)
                    .ThenInclude(rm => rm.IdRequisitosNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.TitulResponsableEvidencia)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == idPostulacionAlumnos, ct)
            ?? throw new NoEncontradoException("Postulación", idPostulacionAlumnos);

        var estado = await _context.TitulPostulacionEstados
            .FirstOrDefaultAsync(e => e.IdPostulacionEstado == idNuevoEstado, ct)
            ?? throw new NoEncontradoException("Estado de postulación", idNuevoEstado);

        if (estado.Nombre != null && (estado.Nombre.Contains("Aprob", StringComparison.OrdinalIgnoreCase) || estado.Nombre.Contains("Acept", StringComparison.OrdinalIgnoreCase)))
        {
            var reqs = postulacion.TitulPostulacionAlumnosRequisitosModalidad.ToList();
            if (reqs.Count > 0)
            {
                var pendientes = reqs.Where(r =>
                {
                    var ultEvidencia = r.TitulResponsableEvidencia?
                        .OrderByDescending(e => e.Actualizado ?? e.Creado)
                        .FirstOrDefault();

                    bool aprobado = ultEvidencia?.Estado == "APROBADO" || r.ValorBool == true;
                    return !aprobado;
                }).ToList();

                if (pendientes.Count > 0)
                {
                    var nombresPendientes = string.Join(", ", pendientes.Select(p => p.IdRequisitoModalidadNavigation?.IdRequisitosNavigation?.Requisito ?? "Requisito"));
                    throw new DominioException($"No se puede aprobar la postulación: faltan requisitos por ser aprobados por los docentes responsables ({nombresPendientes}).");
                }
            }
        }

        postulacion.IdPostulacionEstado = idNuevoEstado;

        await _context.SaveChangesAsync(ct);
    }

    public async Task SolicitarCambioModalidadAsync(
        int idPostulacionAlumnos,
        int idNuevaModalidadTitulacionCarrera,
        CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == idPostulacionAlumnos, ct)
            ?? throw new NoEncontradoException("Postulación", idPostulacionAlumnos);

        postulacion.IdModalidadTitulacionCarrera = idNuevaModalidadTitulacionCarrera;
        postulacion.EsCambioModalidad = true;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<PortalEstudianteDto> ObtenerPortalEstudianteAsync(string idAlumno, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // 1. Obtener elegibilidad base
        var elegibilidad = await ObtenerElegibilidadEstudianteAsync(idAlumno, ct);

        // 2. Obtener alumno y datos de contacto
        var alumno = await _context.Alumnos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdAlumno == idAlumno, ct);

        // 3. Obtener información de la cohorte activa
        var cohorte = await _context.TitulCohortes
            .AsNoTracking()
            .Where(c => c.EsActivo == true)
            .OrderByDescending(c => c.IdCohorte)
            .FirstOrDefaultAsync(ct);

        bool estaAbierta = cohorte != null &&
                           cohorte.EsActivo == true &&
                           (!cohorte.FechaInicio.HasValue || cohorte.FechaInicio.Value <= now) &&
                           (!cohorte.FechaFin.HasValue || cohorte.FechaFin.Value >= now);

        int? diasRestantes = (cohorte?.FechaFin.HasValue == true && cohorte.FechaFin.Value >= now)
            ? (int)Math.Ceiling((cohorte.FechaFin.Value - now).TotalDays)
            : null;

        string mensajeConvocatoria = estaAbierta
            ? $"Convocatoria '{cohorte?.Detelle}' abierta hasta el {cohorte?.FechaFin:dd/MM/yyyy}. ({diasRestantes} días restantes)."
            : (cohorte == null
                ? "No existe una convocatoria de titulación activa actualmente."
                : (cohorte.FechaInicio.HasValue && cohorte.FechaInicio.Value > now
                    ? $"La convocatoria iniciará el {cohorte.FechaInicio:dd/MM/yyyy}."
                    : $"El período de postulaciones cerró el {cohorte.FechaFin:dd/MM/yyyy}."));

        var convocatoriaDto = new ConvocatoriaPortalDto(
            EstaAbierta: estaAbierta,
            Periodo: cohorte?.IdPeriodo,
            Detalle: cohorte?.Detelle,
            FechaInicio: cohorte?.FechaInicio,
            FechaCierre: cohorte?.FechaFin,
            DiasRestantes: diasRestantes,
            Mensaje: mensajeConvocatoria
        );

        var estudianteDto = new EstudiantePortalDto(
            IdAlumno: idAlumno,
            Cedula: idAlumno,
            NombreCompleto: elegibilidad.NombreCompleto ?? $"{alumno?.PrimerNombre} {alumno?.ApellidoPaterno}".Trim(),
            Email: alumno?.EmailInstitucional ?? alumno?.Email,
            Celular: alumno?.Celular ?? alumno?.Telefono,
            IdCarrera: elegibilidad.IdCarrera,
            NombreCarrera: elegibilidad.NombreCarrera,
            IdMatricula: elegibilidad.IdMatricula,
            EsElegible: elegibilidad.EsElegible && estaAbierta,
            MensajeElegibilidad: !estaAbierta ? "El período de postulaciones está cerrado." : (elegibilidad.Mensaje ?? string.Empty)
        );

        // 4. Si ya tiene postulación activa, cargar su detalle
        PostulacionDetalleDto? postulacionActiva = null;
        if (elegibilidad.TienePostulacionActiva && elegibilidad.IdPostulacionActiva.HasValue)
        {
            postulacionActiva = await ObtenerPorIdAsync(elegibilidad.IdPostulacionActiva.Value, ct);
        }

        return new PortalEstudianteDto(
            Convocatoria: convocatoriaDto,
            Estudiante: estudianteDto,
            PostulacionActiva: postulacionActiva,
            ModalidadesDisponibles: elegibilidad.ModalidadesOfertadas
        );
    }

    public async Task DictaminarPostulacionAsync(DictamenPostulacionComando comando, CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.IdRequisitoModalidadNavigation)
                    .ThenInclude(rm => rm.IdRequisitosNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.TitulResponsableEvidencia)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == comando.IdPostulacionAlumnos, ct)
            ?? throw new NoEncontradoException("Postulación", comando.IdPostulacionAlumnos);

        var estados = await _context.TitulPostulacionEstados
            .Where(e => e.EsActivo == true)
            .OrderBy(e => e.Orden)
            .ToListAsync(ct);

        int idNuevoEstado = comando.Decision.ToUpperInvariant() switch
        {
            "APROBAR" => estados.FirstOrDefault(e => e.Nombre != null && (e.Nombre.Contains("Aprob") || e.Nombre.Contains("Acept")))?.IdPostulacionEstado ?? 4,
            "OBSERVAR" => estados.FirstOrDefault(e => e.Nombre != null && (e.Nombre.Contains("Observ") || e.Nombre.Contains("Revis")))?.IdPostulacionEstado ?? 3,
            "RECHAZAR" => estados.FirstOrDefault(e => e.Nombre != null && (e.Nombre.Contains("Rechaz") || e.Nombre.Contains("Negad")))?.IdPostulacionEstado ?? 5,
            _ => throw new ArgumentException($"Decisión no reconocida: {comando.Decision}")
        };

        if (comando.Decision.Equals("APROBAR", StringComparison.OrdinalIgnoreCase))
        {
            var reqs = postulacion.TitulPostulacionAlumnosRequisitosModalidad.ToList();
            if (reqs.Count == 0)
            {
                throw new DominioException("La postulación no cuenta con requisitos configurados para ser evaluados.");
            }

            var pendientes = reqs.Where(r =>
            {
                var ultEvidencia = r.TitulResponsableEvidencia?
                    .OrderByDescending(e => e.Actualizado ?? e.Creado)
                    .FirstOrDefault();

                bool aprobado = ultEvidencia?.Estado == "APROBADO" || r.ValorBool == true;
                return !aprobado;
            }).ToList();

            if (pendientes.Count > 0)
            {
                var nombresPendientes = string.Join(", ", pendientes.Select(p => p.IdRequisitoModalidadNavigation?.IdRequisitosNavigation?.Requisito ?? "Requisito"));
                throw new DominioException($"No se puede aprobar la postulación: faltan requisitos por ser aprobados por los docentes responsables ({nombresPendientes}).");
            }
        }

        if (comando.Decision.Equals("RECHAZAR", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(comando.Observaciones))
        {
            var obsTexto = comando.Observaciones.Trim();
            var reqs = postulacion.TitulPostulacionAlumnosRequisitosModalidad.ToList();
            foreach (var req in reqs)
            {
                var ultEvidencia = req.TitulResponsableEvidencia?
                    .OrderByDescending(e => e.Actualizado ?? e.Creado)
                    .FirstOrDefault();

                if (ultEvidencia != null)
                {
                    ultEvidencia.Estado = "RECHAZADO";
                    ultEvidencia.Observaciones = obsTexto;
                    ultEvidencia.Actualizado = DateTime.UtcNow;
                    ultEvidencia.IdActualizado = "ADMIN";
                }
                else
                {
                    int idReq = req.IdRequisitoModalidadNavigation?.IdRequisitos ?? 0;
                    if (idReq == 0 && req.IdRequisitoModalidad > 0)
                    {
                        idReq = await _context.TitulRequisitoModalidad
                            .Where(rm => rm.IdRequisitoModalidad == req.IdRequisitoModalidad)
                            .Select(rm => rm.IdRequisitos)
                            .FirstOrDefaultAsync(ct);
                    }

                    if (idReq > 0)
                    {
                        var resp = await _context.TitulResponsableRequisitos
                            .FirstOrDefaultAsync(tr => tr.IdRequisitos == idReq, ct);

                        if (resp == null)
                        {
                            var primerProfesor = await _context.Profesores
                                .Where(p => p.Activo == true)
                                .Select(p => p.IdProfesor)
                                .FirstOrDefaultAsync(ct);

                            if (primerProfesor != null)
                            {
                                resp = new TitulResponsableRequisitos
                                {
                                    IdRequisitos = idReq,
                                    IdProfesor = primerProfesor
                                };
                                _context.TitulResponsableRequisitos.Add(resp);
                                await _context.SaveChangesAsync(ct);
                            }
                        }

                        if (resp != null)
                        {
                            _context.TitulResponsableEvidencia.Add(new TitulResponsableEvidencia
                            {
                                IdPostulacionAlumnoRequisitoModalidad = req.IdPostulacionAlumnoRequisitoModalidad,
                                IdResponsableEvidencias = resp.IdResponsableEvidencias,
                                Estado = "RECHAZADO",
                                Observaciones = obsTexto,
                                Creado = DateTime.UtcNow,
                                IdCreado = "ADMIN"
                            });
                        }
                    }
                }
            }
        }

        postulacion.IdPostulacionEstado = idNuevoEstado;

        await _context.SaveChangesAsync(ct);
    }

    private static PostulacionDetalleDto MapearDetalle(TitulPostulacionAlumnos p)
    {
        var alumno = p.IdMatriculaNavigation?.IdAlumnoNavigation;
        var carrera = p.IdMatriculaNavigation?.IdNivelNavigation?.IdCarreraNavigation;
        var cohorte = p.IdModalidadTitulacionCarreraNavigation?.IdCohorteCarreraNavigation?.IdCohorteNavigation;
        var modalidad = p.IdModalidadTitulacionCarreraNavigation?.IdModalidadTitulacionNavigation;

        var nombreCompleto = $"{alumno?.PrimerNombre} {alumno?.SegundoNombre} {alumno?.ApellidoPaterno} {alumno?.ApellidoMaterno}".Replace("  ", " ").Trim();

        var requisitos = p.TitulPostulacionAlumnosRequisitosModalidad
            .Select(r =>
            {
                var ultEvidencia = r.TitulResponsableEvidencia?
                    .OrderByDescending(e => e.Actualizado ?? e.Creado)
                    .FirstOrDefault();

                string estadoValidacion = ultEvidencia?.Estado ?? (r.ValorBool == true ? "APROBADO" : "PENDIENTE");
                string? observaciones = ultEvidencia?.Observaciones;

                var profesorNav = ultEvidencia?.IdResponsableEvidenciasNavigation?.IdProfesorNavigation;
                string? nombreEvaluador = profesorNav != null
                    ? $"{profesorNav.Nombres} {profesorNav.Apellidos}".Trim()
                    : null;
                string? cedulaEvaluador = ultEvidencia?.IdResponsableEvidenciasNavigation?.IdProfesor ?? ultEvidencia?.IdActualizado ?? ultEvidencia?.IdCreado;
                DateTime? fechaEvaluacion = ultEvidencia?.Actualizado ?? ultEvidencia?.Creado;

                return new PostulacionRequisitoDetalleDto(
                    IdPostulacionAlumnoRequisitoModalidad: r.IdPostulacionAlumnoRequisitoModalidad,
                    IdPostulacionAlumnos: r.IdPostulacionAlumnos,
                    IdRequisitoModalidad: r.IdRequisitoModalidad,
                    IdRequisitos: r.IdRequisitoModalidadNavigation?.IdRequisitos ?? 0,
                    NombreRequisito: r.IdRequisitoModalidadNavigation?.IdRequisitosNavigation?.Requisito ?? string.Empty,
                    EsAdjunto: r.IdRequisitoModalidadNavigation?.IdRequisitosNavigation?.EsAdjunto == true,
                    EsBool: r.IdRequisitoModalidadNavigation?.IdRequisitosNavigation?.EsBool == true,
                    SubeAlumno: r.IdRequisitoModalidadNavigation?.IdRequisitosNavigation?.SubeAlumno == true,
                    IdAdjuntosImagenes: r.IdAdjuntosImagenes,
                    NombreArchivoAdjunto: r.IdAdjuntosImagenesNavigation?.NombreArchivos,
                    RutaArchivoAdjunto: r.IdAdjuntosImagenesNavigation?.Ruta,
                    ValorBool: r.ValorBool,
                    EstadoValidacion: estadoValidacion,
                    Observaciones: observaciones,
                    NombreEvaluador: nombreEvaluador,
                    CedulaEvaluador: cedulaEvaluador,
                    FechaEvaluacion: fechaEvaluacion
                );
            })
            .ToList();

        string? observacionDictamen = p.TitulPostulacionAlumnosRequisitosModalidad
            .SelectMany(r => r.TitulResponsableEvidencia ?? Enumerable.Empty<TitulResponsableEvidencia>())
            .Where(e => !string.IsNullOrWhiteSpace(e.Observaciones))
            .OrderByDescending(e => e.Actualizado ?? e.Creado)
            .Select(e => e.Observaciones)
            .FirstOrDefault();

        return new PostulacionDetalleDto(
            IdPostulacionAlumnos: p.IdPostulacionAlumnos,
            IdMatricula: p.IdMatricula,
            IdAlumno: alumno?.IdAlumno ?? string.Empty,
            NombreAlumno: nombreCompleto,
            CedulaAlumno: alumno?.IdAlumno ?? string.Empty,
            EmailAlumno: alumno?.EmailInstitucional ?? alumno?.Email ?? string.Empty,
            TelefonoAlumno: alumno?.Celular ?? alumno?.Telefono ?? string.Empty,
            IdCarrera: p.IdMatriculaNavigation?.IdNivelNavigation?.IdCarrera ?? 0,
            NombreCarrera: carrera?.Carrera ?? string.Empty,
            IdCohorte: cohorte?.IdCohorte ?? 0,
            DetalleCohorte: cohorte?.Detelle ?? string.Empty,
            IdModalidadTitulacionCarrera: p.IdModalidadTitulacionCarrera,
            ModalidadTitulacion: modalidad?.ModalidadTitulacion ?? string.Empty,
            IdPostulacionEstado: p.IdPostulacionEstado,
            NombreEstado: p.IdPostulacionEstadoNavigation?.Nombre ?? string.Empty,
            EsActivo: p.EsActivo,
            EsCambioModalidad: p.EsCambioModalidad,
            Requisitos: requisitos,
            ObservacionDictamen: observacionDictamen
        );
    }
}
