using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public sealed class RepositorioPostulaciones(SigafiDbContext context, IEmailService emailService) : IRepositorioPostulaciones
{
    private readonly SigafiDbContext _context = context;
    private readonly IEmailService _emailService = emailService;

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

        // Buscar matrícula vigente con su nivel y carrera, priorizando el nivel académico más alto
        var matricula = await _context.Matriculas
            .AsNoTracking()
            .Include(m => m.IdNivelNavigation)
                .ThenInclude(n => n.IdCarreraNavigation)
            .Include(m => m.IdModalidadNavigation)
            .Where(m => m.IdAlumno == idAlumno && (m.Retirado == false || m.Retirado == null))
            .OrderByDescending(m => m.IdNivelNavigation != null ? (m.IdNivelNavigation.Orden ?? m.IdNivelNavigation.Jerarquia ?? 0) : 0)
            .ThenByDescending(m => m.FechaMatricula ?? DateTime.MinValue)
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
            DetalleCohorte: cohorteCarrera?.IdCohorteNavigation?.Detalle,
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
                    EsRequisitoFinal: rm.EsRequisitoFinal == true
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
            .AsSplitQuery()
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

        var detalle = MapearDetalle(postulacion);
        if (!string.IsNullOrEmpty(detalle.IdAlumno))
        {
            var (noAdeuda, saldo, tieneRestricciones) = await ConsultarFinanzasAlumnoAsync(detalle.IdAlumno, detalle.IdCarrera > 0 ? detalle.IdCarrera : null, ct);
            detalle = detalle with
            {
                NoAdeuda = noAdeuda,
                SaldoPendiente = saldo,
                TieneRestricciones = tieneRestricciones
            };
        }
        return detalle;
    }

    public async Task<PostulacionDetalleDto?> ObtenerPorIdAsync(
        int idPostulacionAlumnos, CancellationToken ct = default)
    {
        await SincronizarRequisitosModalidadAsync(idPostulacionAlumnos, ct);

        var postulacion = await _context.TitulPostulacionAlumnos
            .AsNoTracking()
            .AsSplitQuery()
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

        var detallePorId = MapearDetalle(postulacion);
        if (!string.IsNullOrEmpty(detallePorId.IdAlumno))
        {
            var (noAdeuda, saldo, tieneRestricciones) = await ConsultarFinanzasAlumnoAsync(detallePorId.IdAlumno, detallePorId.IdCarrera > 0 ? detallePorId.IdCarrera : null, ct);
            detallePorId = detallePorId with
            {
                NoAdeuda = noAdeuda,
                SaldoPendiente = saldo,
                TieneRestricciones = tieneRestricciones
            };
        }
        return detallePorId;
    }

    private async Task<(bool noAdeuda, decimal saldoPendiente, bool tieneRestricciones)> ConsultarFinanzasAlumnoAsync(string idAlumno, int? idCarrera, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idAlumno))
        {
            return (true, 0, false);
        }

        var queryMatriculas = _context.Matriculas
            .AsNoTracking()
            .Where(m => m.IdAlumno == idAlumno);

        if (idCarrera.HasValue && idCarrera.Value > 0)
        {
            queryMatriculas = queryMatriculas.Where(m => m.IdNivelNavigation != null && m.IdNivelNavigation.IdCarrera == idCarrera.Value);
        }

        var matriculaIds = await queryMatriculas
            .Select(m => m.IdMatricula)
            .ToListAsync(ct);

        decimal saldo = 0;
        if (matriculaIds.Count > 0)
        {
            var creditos = await _context.CreditoAlumno
                .AsNoTracking()
                .Where(c => matriculaIds.Contains(c.IdMatricula) && c.Saldo > 0)
                .Select(c => new { c.Saldo, c.CreditoInicial })
                .ToListAsync(ct);

            saldo = creditos.Sum(x => x.CreditoInicial.HasValue && x.CreditoInicial.Value > 0
                ? Math.Min(x.Saldo ?? 0, x.CreditoInicial.Value)
                : (x.Saldo ?? 0));
        }

        bool noAdeuda = saldo <= 0;
        return (noAdeuda, saldo, false);
    }

    private async Task SincronizarRequisitosModalidadAsync(int idPostulacionAlumnos, CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.TitulResponsableEvidencia)
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
            .Where(rm => rm.IdModalidadTitulacion == idModalidad && rm.EsActivo == true && rm.IdRequisitosNavigation.EsActivo == true)
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

        IOrderedQueryable<TitulPostulacionAlumnos> orderedQuery;
        if (!idEstado.HasValue)
        {
            // Al consultar todas, priorizar primero las postulaciones "EN REVISIÓN" (orden de estado menor)
            orderedQuery = query
                .OrderBy(p => p.IdPostulacionEstadoNavigation != null ? p.IdPostulacionEstadoNavigation.Orden : p.IdPostulacionEstado)
                .ThenByDescending(p => p.IdPostulacionAlumnos);
        }
        else
        {
            orderedQuery = query.OrderByDescending(p => p.IdPostulacionAlumnos);
        }

        var items = await orderedQuery
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
                p.IdModalidadTitulacionCarreraNavigation.IdCohorteCarreraNavigation.IdCohorteNavigation.Detalle ?? string.Empty,
                p.IdModalidadTitulacionCarrera,
                p.IdModalidadTitulacionCarreraNavigation.IdModalidadTitulacionNavigation.ModalidadTitulacion ?? string.Empty,
                p.IdPostulacionEstado,
                p.IdPostulacionEstadoNavigation.Nombre ?? string.Empty,
                p.EsActivo,
                p.EsCambioModalidad,
                p.TitulPostulacionAlumnosRequisitosModalidad.Count(r => r.IdRequisitoModalidadNavigation.EsActivo == true && (r.IdRequisitoModalidadNavigation.IdRequisitosNavigation == null || r.IdRequisitoModalidadNavigation.IdRequisitosNavigation.EsActivo == true)),
                p.TitulPostulacionAlumnosRequisitosModalidad.Count(r => (r.IdAdjuntosImagenes != null || r.ValorBool == true) && r.IdRequisitoModalidadNavigation.EsActivo == true && (r.IdRequisitoModalidadNavigation.IdRequisitosNavigation == null || r.IdRequisitoModalidadNavigation.IdRequisitosNavigation.EsActivo == true)),
                null,
                null,
                null
            ))
            .ToListAsync(ct);

        if (items.Count > 0)
        {
            var studentCarreraPairs = items
                .Select(p => new { p.IdAlumno, p.IdCarrera })
                .Where(x => !string.IsNullOrEmpty(x.IdAlumno))
                .Distinct()
                .ToList();

            var studentIds = studentCarreraPairs.Select(x => x.IdAlumno).Distinct().ToList();

            var matriculasEstudiantes = await _context.Matriculas
                .AsNoTracking()
                .Where(m => studentIds.Contains(m.IdAlumno))
                .Select(m => new { m.IdAlumno, m.IdMatricula, IdCarrera = m.IdNivelNavigation != null ? m.IdNivelNavigation.IdCarrera : 0 })
                .ToListAsync(ct);

            var validMatriculas = matriculasEstudiantes
                .Where(m => studentCarreraPairs.Any(sc => sc.IdAlumno == m.IdAlumno && (sc.IdCarrera == 0 || sc.IdCarrera == m.IdCarrera)))
                .ToList();

            var matriculaIdToAlumnoKey = validMatriculas.ToDictionary(m => m.IdMatricula, m => $"{m.IdAlumno}_{m.IdCarrera}");
            var allMatriculaIds = validMatriculas.Select(m => m.IdMatricula).ToList();

            var saldosPendientes = await _context.CreditoAlumno
                .AsNoTracking()
                .Where(c => allMatriculaIds.Contains(c.IdMatricula) && c.Saldo > 0)
                .Select(c => new { c.IdMatricula, c.Saldo, c.CreditoInicial })
                .ToListAsync(ct);

            var deudasPorAlumnoCarrera = saldosPendientes
                .GroupBy(s => matriculaIdToAlumnoKey[s.IdMatricula])
                .ToDictionary(g => g.Key, g => g.Sum(x => x.CreditoInicial.HasValue && x.CreditoInicial.Value > 0
                    ? Math.Min(x.Saldo ?? 0, x.CreditoInicial.Value)
                    : (x.Saldo ?? 0)));

            items = items.Select(p =>
            {
                var key = $"{p.IdAlumno}_{p.IdCarrera}";
                var saldo = deudasPorAlumnoCarrera.GetValueOrDefault(key, 0);
                var noAdeuda = saldo <= 0;
                return p with
                {
                    NoAdeuda = noAdeuda,
                    SaldoPendiente = saldo,
                    TieneRestricciones = false
                };
            }).ToList();
        }

        return new PaginaPostulacionesDto(items, pagina, tamanoPagina, total);
    }

    public async Task<int> ContarTotalPostulacionesAsync(CancellationToken ct = default)
    {
        return await _context.TitulPostulacionAlumnos
            .AsNoTracking()
            .CountAsync(ct);
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

        // Verificar que no tenga otra postulación activa en curso o aprobada (que no esté rechazada)
        bool tieneActiva = await _context.TitulPostulacionAlumnos
            .Include(p => p.IdPostulacionEstadoNavigation)
            .AnyAsync(p => p.IdMatriculaNavigation.IdAlumno == matricula.IdAlumno
                           && p.EsActivo == true
                           && (p.IdPostulacionEstadoNavigation == null ||
                               (!p.IdPostulacionEstadoNavigation.Nombre.Contains("Rechaz") &&
                                !p.IdPostulacionEstadoNavigation.Nombre.Contains("Negad"))), ct);

        if (tieneActiva)
        {
            throw new InvalidOperationException("El estudiante ya posee una postulación activa en el sistema.");
        }

        // Si el estudiante tenía postulaciones anteriores rechazadas, desactivarlas para que la nueva sea la única vigente
        var postulacionesAnterioresRechazadas = await _context.TitulPostulacionAlumnos
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Where(p => p.IdMatriculaNavigation.IdAlumno == matricula.IdAlumno
                        && p.EsActivo == true
                        && p.IdPostulacionEstadoNavigation != null
                        && (p.IdPostulacionEstadoNavigation.Nombre.Contains("Rechaz") ||
                            p.IdPostulacionEstadoNavigation.Nombre.Contains("Negad")))
            .ToListAsync(ct);

        foreach (var previa in postulacionesAnterioresRechazadas)
        {
            previa.EsActivo = false;
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

        // Extraer de forma segura los datos para la notificación por correo ANTES del hilo secundario,
        // evitando accesos concurrentes al DbContext (_context) que causan InvalidOperationException / 500.
        string? correoEstudiante = null;
        string nombreEstudiante = string.Empty;
        string nombreCarreraNotif = "Carrera ISTPET";
        string nombreModalidadNotif = "Modalidad de Titulación";
        int idNuevaPostulacion = postulacion.IdPostulacionAlumnos;

        try
        {
            var alumnoObj = await _context.Alumnos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAlumno == matricula.IdAlumno, ct);

            correoEstudiante = alumnoObj?.EmailInstitucional ?? alumnoObj?.Email;
            nombreEstudiante = $"{alumnoObj?.PrimerNombre} {alumnoObj?.ApellidoPaterno}".Trim();

            var matriculaConCarrera = await _context.Matriculas
                .AsNoTracking()
                .Include(m => m.IdNivelNavigation)
                    .ThenInclude(n => n.IdCarreraNavigation)
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatricula, ct);

            nombreCarreraNotif = matriculaConCarrera?.IdNivelNavigation?.IdCarreraNavigation?.Carrera ?? "Carrera ISTPET";

            nombreModalidadNotif = await _context.TitulModalidadesTitulacionCarreras
                .AsNoTracking()
                .Include(m => m.IdModalidadTitulacionNavigation)
                .Where(m => m.IdModalidadTitulacionCarrera == idModalidadTitulacionCarrera)
                .Select(m => m.IdModalidadTitulacionNavigation.ModalidadTitulacion)
                .FirstOrDefaultAsync(ct) ?? "Modalidad de Titulación";
        }
        catch
        {
            // El fallo en la captura de metadatos de correo no debe impedir registrar la postulación
        }

        if (!string.IsNullOrWhiteSpace(correoEstudiante))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.NotificarPostulacionRegistradaAsync(
                        correoEstudiante, nombreEstudiante, nombreCarreraNotif, nombreModalidadNotif, idNuevaPostulacion, CancellationToken.None);
                }
                catch
                {
                    // El envío en segundo plano no interrumpe el flujo principal
                }
            }, CancellationToken.None);
        }

        return idNuevaPostulacion;
    }

    public async Task ActualizarRequisitosAsync(
        int idPostulacionAlumnos,
        IReadOnlyList<RequisitoPostulacionInputDto> requisitos,
        CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.TitulResponsableEvidencia)
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
                    // Regla 1: Un requisito previamente aprobado no puede volver a subirse ni reemplazarse
                    bool yaAprobado = existente.ValorBool == true ||
                        (existente.TitulResponsableEvidencia != null && existente.TitulResponsableEvidencia.Any(e => e.Estado == "APROBADO"));
                    if (yaAprobado)
                    {
                        throw new InvalidOperationException("No se puede reemplazar el documento de un requisito que ya ha sido aprobado.");
                    }

                    // Regla 2: Si ya tenía un documento subido previamente, solo puede reemplazarse si el docente lo rechazó u observó
                    bool yaTieneDocumento = existente.IdAdjuntosImagenes.HasValue && existente.IdAdjuntosImagenes.Value > 0;
                    if (yaTieneDocumento)
                    {
                        bool fueRechazadoUObservado = existente.TitulResponsableEvidencia != null &&
                            existente.TitulResponsableEvidencia.Any(e => e.Estado == "RECHAZADO" || e.Estado == "OBSERVADO");

                        if (!fueRechazadoUObservado)
                        {
                            throw new InvalidOperationException("El documento ya fue subido y se encuentra en revisión. No se puede reemplazar hasta que sea evaluado.");
                        }
                    }

                    existente.IdAdjuntosImagenes = idAdjunto.Value;
                    // Al subir documento, no auto-aprobar; debe quedar pendiente de evaluación docente
                    existente.ValorBool = reqInput.ValorBool;

                    if (existente.TitulResponsableEvidencia != null && existente.TitulResponsableEvidencia.Count > 0)
                    {
                        foreach (var ev in existente.TitulResponsableEvidencia.Where(e => e.Estado != "APROBADO"))
                        {
                            ev.Estado = "PENDIENTE";
                            ev.Actualizado = DateTime.UtcNow;
                        }
                    }
                }
                else if (reqInput.ValorBool.HasValue)
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

        postulacion.IdPostulacionEstado = idNuevoEstado;

        await _context.SaveChangesAsync(ct);
    }

    public async Task SolicitarCambioModalidadAsync(
        int idPostulacionAlumnos,
        int idNuevaModalidadTitulacionCarrera,
        CancellationToken ct = default)
    {
        var postulacion = await _context.TitulPostulacionAlumnos
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdNivelNavigation)
                    .ThenInclude(n => n.IdCarreraNavigation)
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.IdRequisitoModalidadNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.TitulResponsableEvidencia)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == idPostulacionAlumnos, ct)
            ?? throw new NoEncontradoException("Postulación", idPostulacionAlumnos);

        var nuevaModalidadCarrera = await _context.TitulModalidadesTitulacionCarreras
            .Include(m => m.IdModalidadTitulacionNavigation)
            .FirstOrDefaultAsync(m => m.IdModalidadTitulacionCarrera == idNuevaModalidadTitulacionCarrera && m.EsActivo == true, ct)
            ?? throw new NoEncontradoException("Modalidad de Titulación de Carrera", idNuevaModalidadTitulacionCarrera);

        // Obtener la cohorte activa y verificar tiempos correctos
        var now = DateTime.UtcNow;
        var cohorte = await _context.TitulCohortes
            .AsNoTracking()
            .Where(c => c.EsActivo == true)
            .OrderByDescending(c => c.IdCohorte)
            .FirstOrDefaultAsync(ct);

        bool convocatoriaAbierta = cohorte != null &&
                                   cohorte.EsActivo == true &&
                                   (!cohorte.FechaInicio.HasValue || cohorte.FechaInicio.Value <= now) &&
                                   (!cohorte.FechaFin.HasValue || cohorte.FechaFin.Value >= now);

        var estadoActualNombre = postulacion.IdPostulacionEstadoNavigation?.Nombre ?? string.Empty;
        bool esRechazada = estadoActualNombre.Contains("Rechaz", StringComparison.OrdinalIgnoreCase) ||
                           estadoActualNombre.Contains("Negad", StringComparison.OrdinalIgnoreCase);

        if (!convocatoriaAbierta && !esRechazada)
        {
            throw new DominioException("No se puede cambiar de modalidad: la convocatoria está cerrada y la postulación no se encuentra en estado rechazada.");
        }

        // Obtener el primer estado activo (ej. 'EN REVISIÓN' o 'REGISTRADA') para reiniciar la evaluación por Coordinación
        var primerEstado = await _context.TitulPostulacionEstados
            .Where(e => e.EsActivo == true)
            .OrderBy(e => e.Orden)
            .FirstOrDefaultAsync(ct);

        if (primerEstado != null)
        {
            postulacion.IdPostulacionEstado = primerEstado.IdPostulacionEstado;
        }

        postulacion.IdModalidadTitulacionCarrera = idNuevaModalidadTitulacionCarrera;
        postulacion.EsCambioModalidad = true;

        // Migración inteligente de requisitos:
        // Cargar los requisitos configurados para la nueva modalidad
        var reqsNuevaModalidad = await _context.TitulRequisitoModalidad
            .Where(rm => rm.IdModalidadTitulacion == nuevaModalidadCarrera.IdModalidadTitulacion && rm.EsActivo)
            .ToListAsync(ct);

        var mapRequisitoBaseANuevaModalidad = reqsNuevaModalidad
            .ToDictionary(rm => rm.IdRequisitos, rm => rm);

        // 1. Conservar requisitos existentes si coinciden en la nueva modalidad, o eliminarlos si ya no aplican.
        // Si un requisito tenía evidencia rechazada de la modalidad anterior, limpiarla para que en la nueva
        // modalidad comience limpio (PENDIENTE), conservando intactos los que ya estaban APROBADOS.
        var requisitosActuales = postulacion.TitulPostulacionAlumnosRequisitosModalidad.ToList();
        foreach (var existente in requisitosActuales)
        {
            var idRequisitoBase = existente.IdRequisitoModalidadNavigation?.IdRequisitos;
            var reqNuevo = (idRequisitoBase != null && mapRequisitoBaseANuevaModalidad.TryGetValue(idRequisitoBase.Value, out var rmNuevo))
                ? rmNuevo
                : null;

            if (reqNuevo != null)
            {
                existente.IdRequisitoModalidad = reqNuevo.IdRequisitoModalidad;

                if (existente.TitulResponsableEvidencia != null && existente.TitulResponsableEvidencia.Count > 0)
                {
                    // Conservar las evidencias y trazabilidad histórica; si cambia de modalidad,
                    // los requisitos previamente aprobados permanecen aprobados y los rechazados/observados
                    // conservan su motivo y auditoría intactos sin eliminarse de la base de datos.
                }
            }
            else
            {
                _context.TitulPostulacionAlumnosRequisitosModalidad.Remove(existente);
            }
        }

        // 2. Agregar requisitos nuevos de la modalidad destino que no existían
        var idsRequisitosAsignados = postulacion.TitulPostulacionAlumnosRequisitosModalidad
            .Select(r => r.IdRequisitoModalidad)
            .ToHashSet();

        foreach (var rm in reqsNuevaModalidad)
        {
            if (!idsRequisitosAsignados.Contains(rm.IdRequisitoModalidad))
            {
                _context.TitulPostulacionAlumnosRequisitosModalidad.Add(new TitulPostulacionAlumnosRequisitosModalidad
                {
                    IdPostulacionAlumnos = postulacion.IdPostulacionAlumnos,
                    IdRequisitoModalidad = rm.IdRequisitoModalidad,
                    IdAdjuntosImagenes = null,
                    ValorBool = false
                });
            }
        }

        await _context.SaveChangesAsync(ct);

        // Notificar al estudiante por correo electrónico sobre el cambio de modalidad registrado
        var aluCambio = postulacion.IdMatriculaNavigation?.IdAlumnoNavigation;
        var correoCambio = aluCambio?.EmailInstitucional ?? aluCambio?.Email;
        if (!string.IsNullOrWhiteSpace(correoCambio))
        {
            var carreraNombreCambio = postulacion.IdMatriculaNavigation?.IdNivelNavigation?.IdCarreraNavigation?.Carrera ?? "Carrera ISTPET";
            var nuevaModNombre = nuevaModalidadCarrera.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? "Nueva Modalidad";
            var nombreEstCambio = $"{aluCambio?.PrimerNombre} {aluCambio?.ApellidoPaterno}".Trim();
            int idPostulacionCambio = postulacion.IdPostulacionAlumnos;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.NotificarCambioModalidadAsync(
                        correoCambio, nombreEstCambio, carreraNombreCambio, nuevaModNombre, idPostulacionCambio, CancellationToken.None);
                }
                catch
                {
                    // El envío en segundo plano no interrumpe el flujo principal
                }
            }, CancellationToken.None);
        }
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
            ? $"Convocatoria '{cohorte?.Detalle}' abierta hasta el {cohorte?.FechaFin:dd/MM/yyyy}. ({diasRestantes} días restantes)."
            : (cohorte == null
                ? "No existe una convocatoria de titulación activa actualmente."
                : (cohorte.FechaInicio.HasValue && cohorte.FechaInicio.Value > now
                    ? $"La convocatoria iniciará el {cohorte.FechaInicio:dd/MM/yyyy}."
                    : $"El período de postulaciones cerró el {cohorte.FechaFin:dd/MM/yyyy}."));

        var convocatoriaDto = new ConvocatoriaPortalDto(
            EstaAbierta: estaAbierta,
            Periodo: cohorte?.IdPeriodo,
            Detalle: cohorte?.Detalle,
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
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdNivelNavigation)
                    .ThenInclude(n => n.IdCarreraNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(p => p.IdPostulacionEstadoNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.IdRequisitoModalidadNavigation)
                    .ThenInclude(rm => rm.IdRequisitosNavigation)
            .Include(p => p.TitulPostulacionAlumnosRequisitosModalidad)
                .ThenInclude(r => r.TitulResponsableEvidencia)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == comando.IdPostulacionAlumnos, ct)
            ?? throw new NoEncontradoException("Postulación", comando.IdPostulacionAlumnos);

        // Si la postulación ya está rechazada o en estado final, no permitir nuevos dictámenes
        if (postulacion.IdPostulacionEstadoNavigation != null &&
            (postulacion.IdPostulacionEstadoNavigation.EsFinal == true ||
             postulacion.IdPostulacionEstadoNavigation.Nombre.Contains("Rechaz", StringComparison.OrdinalIgnoreCase) ||
             postulacion.IdPostulacionEstadoNavigation.Nombre.Contains("Negad", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"La postulación ya se encuentra en estado rechazado o final ({postulacion.IdPostulacionEstadoNavigation.Nombre}) y no permite nuevos dictámenes.");
        }

        var estados = await _context.TitulPostulacionEstados
            .Where(e => e.EsActivo == true)
            .OrderBy(e => e.Orden)
            .ToListAsync(ct);

        int idNuevoEstado = comando.Decision.ToUpperInvariant() switch
        {
            "APROBAR" => estados.FirstOrDefault(e => e.Nombre != null && (e.Nombre.Contains("Aprob", StringComparison.OrdinalIgnoreCase) || e.Nombre.Contains("Acept", StringComparison.OrdinalIgnoreCase)))?.IdPostulacionEstado ?? 2,
            "OBSERVAR" => estados.FirstOrDefault(e => e.Nombre != null && (e.Nombre.Contains("Observ", StringComparison.OrdinalIgnoreCase) || e.Nombre.Contains("Revis", StringComparison.OrdinalIgnoreCase)))?.IdPostulacionEstado ?? 3,
            "RECHAZAR" => estados.FirstOrDefault(e => e.Nombre != null && (e.Nombre.Contains("Rechaz", StringComparison.OrdinalIgnoreCase) || e.Nombre.Contains("Negad", StringComparison.OrdinalIgnoreCase)))?.IdPostulacionEstado ?? 4,
            _ => throw new ArgumentException($"Decisión no reconocida: {comando.Decision}")
        };

        // Al rechazar la postulación solo se rechaza la postulación misma, NUNCA los requisitos.
        // Solo en caso de OBSERVAR se actualizan los requisitos específicos que hayan sido seleccionados.
        if (comando.Decision.Equals("OBSERVAR", StringComparison.OrdinalIgnoreCase) &&
            comando.IdsRequisitosObservados != null &&
            comando.IdsRequisitosObservados.Count > 0 &&
            !string.IsNullOrWhiteSpace(comando.Observaciones))
        {
            var obsTexto = comando.Observaciones.Trim();
            var setIds = comando.IdsRequisitosObservados.ToHashSet();
            var reqs = postulacion.TitulPostulacionAlumnosRequisitosModalidad
                .Where(r => setIds.Contains(r.IdPostulacionAlumnoRequisitoModalidad) || setIds.Contains(r.IdRequisitoModalidad))
                .ToList();

            foreach (var req in reqs)
            {
                var ultEvidencia = req.TitulResponsableEvidencia?
                    .OrderByDescending(e => e.Actualizado ?? e.Creado)
                    .FirstOrDefault();

                if (ultEvidencia != null)
                {
                    ultEvidencia.Estado = "OBSERVADO";
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
                                Estado = "OBSERVADO",
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

        // Notificar al estudiante por correo electrónico sobre el dictamen oficial emitido por Coordinación
        var aluDictamen = postulacion.IdMatriculaNavigation?.IdAlumnoNavigation;
        var correoDictamen = aluDictamen?.EmailInstitucional ?? aluDictamen?.Email;
        if (!string.IsNullOrWhiteSpace(correoDictamen))
        {
            var carreraNombreDictamen = postulacion.IdMatriculaNavigation?.IdNivelNavigation?.IdCarreraNavigation?.Carrera ?? "Carrera ISTPET";
            var modNombreDictamen = postulacion.IdModalidadTitulacionCarreraNavigation?.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? "Modalidad de Titulación";
            var nombreEstDictamen = $"{aluDictamen?.PrimerNombre} {aluDictamen?.ApellidoPaterno}".Trim();
            int idPostulacionDictamen = postulacion.IdPostulacionAlumnos;
            var decisionDictamen = comando.Decision;
            var observacionesDictamen = comando.Observaciones;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.NotificarDictamenPostulacionAsync(
                        correoDictamen, nombreEstDictamen, carreraNombreDictamen, modNombreDictamen, idPostulacionDictamen, decisionDictamen, observacionesDictamen, CancellationToken.None);
                }
                catch
                {
                    // El envío en segundo plano no interrumpe el flujo principal
                }
            }, CancellationToken.None);
        }
    }

    private static PostulacionDetalleDto MapearDetalle(TitulPostulacionAlumnos p)
    {
        var alumno = p.IdMatriculaNavigation?.IdAlumnoNavigation;
        var carrera = p.IdMatriculaNavigation?.IdNivelNavigation?.IdCarreraNavigation;
        var cohorte = p.IdModalidadTitulacionCarreraNavigation?.IdCohorteCarreraNavigation?.IdCohorteNavigation;
        var modalidad = p.IdModalidadTitulacionCarreraNavigation?.IdModalidadTitulacionNavigation;

        var nombreCompleto = $"{alumno?.PrimerNombre} {alumno?.SegundoNombre} {alumno?.ApellidoPaterno} {alumno?.ApellidoMaterno}".Replace("  ", " ").Trim();

        var requisitos = p.TitulPostulacionAlumnosRequisitosModalidad
            .Where(r => r.IdRequisitoModalidadNavigation == null ||
                        (r.IdRequisitoModalidadNavigation.EsActivo == true &&
                         (r.IdRequisitoModalidadNavigation.IdRequisitosNavigation == null || r.IdRequisitoModalidadNavigation.IdRequisitosNavigation.EsActivo == true)))
            .Select(r =>
            {
                var ultEvidencia = r.TitulResponsableEvidencia?
                    .OrderByDescending(e => e.Actualizado ?? e.Creado)
                    .ThenByDescending(e => e.IdTitulResponsableEvidencia)
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
                    FechaEvaluacion: fechaEvaluacion,
                    EsRequisitoFinal: r.IdRequisitoModalidadNavigation?.EsRequisitoFinal == true
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
            DetalleCohorte: cohorte?.Detalle ?? string.Empty,
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
