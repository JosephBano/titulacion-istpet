using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TitulacionIstpet.Application.DTOs.Egresados;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public sealed class EgresadosService(SigafiDbContext context, IMemoryCache cache) : IEgresadosService
{
    private readonly SigafiDbContext _context = context;
    private readonly IMemoryCache _cache = cache;

    public async Task<PagedResultDto<EstudiantePendienteEgresoDto>> GetPendientesEgresoAsync(
        FiltroPendientesEgresoRequestDto filtro,
        CancellationToken cancellationToken = default)
    {
        bool esBusquedaPuntual = !string.IsNullOrWhiteSpace(filtro.CedulaOrNombre);

        // 1. Periodo activo (solo si no hay filtro explícito y no es búsqueda puntual, con caché)
        string? periodoFiltro = filtro.IdPeriodo?.Trim();
        if (!esBusquedaPuntual && string.IsNullOrWhiteSpace(periodoFiltro))
        {
            periodoFiltro = await _cache.GetOrCreateAsync("periodo_activo_instituto", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await _context.Periodos
                    .AsNoTracking()
                    .Where(p => (p.Periodoactivoinstituto == true || p.Activo == true)
                             && (p.EsConduccion == null || p.EsConduccion == false))
                    .OrderByDescending(p => p.Periodoactivoinstituto == true)
                    .ThenByDescending(p => p.FechaInicial)
                    .Select(p => p.IdPeriodo)
                    .FirstOrDefaultAsync(cancellationToken);
            });
        }

        string cacheKey = $"pendientes_egreso_{periodoFiltro}_{filtro.IdCarrera}_{filtro.IdModalidad}_{filtro.CedulaOrNombre?.Trim().ToUpper()}";

        var resultados = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            // 2. Query base acotado — pre-filtra niveles tempranos (< 3) en listados masivos
            var queryMatriculas = _context.Matriculas
                .AsNoTracking()
                .Where(m => m.Retirado == null || m.Retirado == false);

            if (!esBusquedaPuntual && !string.IsNullOrWhiteSpace(periodoFiltro))
            {
                queryMatriculas = queryMatriculas.Where(m => m.IdPeriodo == periodoFiltro);
            }

            if (filtro.IdModalidad.HasValue)
            {
                queryMatriculas = queryMatriculas.Where(m => m.IdModalidad == filtro.IdModalidad.Value);
            }

            var alumnosBaseQuery = from mat in queryMatriculas
                                   join a in _context.Alumnos on mat.IdAlumno equals a.IdAlumno
                                   join cur in _context.Cursos on mat.IdNivel equals cur.IdNivel
                                   join c in _context.Carreras on cur.IdCarrera equals c.IdCarrera
                                   join p in _context.Periodos on mat.IdPeriodo equals p.IdPeriodo into pGroup
                                   from p in pGroup.DefaultIfEmpty()
                                   where (c.EsInstituto == null || c.EsInstituto == true)
                                      && (p == null || p.EsConduccion == null || p.EsConduccion == false)
                                      && (esBusquedaPuntual || cur.Orden >= 3)
                                   select new
                                   {
                                       mat.IdMatricula,
                                       mat.IdAlumno,
                                       mat.IdPeriodo,
                                       mat.IdModalidad,
                                       mat.FechaMatricula,
                                       mat.IdNivel,
                                       NivelNombre = cur.Nivel,
                                       cur.Orden,
                                       a.PrimerNombre,
                                       a.SegundoNombre,
                                       a.ApellidoPaterno,
                                       a.ApellidoMaterno,
                                       a.Email,
                                       a.Celular,
                                       c.IdCarrera,
                                       c.Carrera
                                   };

            if (filtro.IdCarrera.HasValue)
            {
                alumnosBaseQuery = alumnosBaseQuery.Where(x => x.IdCarrera == filtro.IdCarrera.Value);
            }

            if (esBusquedaPuntual)
            {
                string busqueda = filtro.CedulaOrNombre!.Trim();
                alumnosBaseQuery = alumnosBaseQuery.Where(x =>
                    EF.Functions.Like(x.IdAlumno, $"%{busqueda}%") ||
                    (x.ApellidoPaterno != null && EF.Functions.Like(x.ApellidoPaterno, $"%{busqueda}%")) ||
                    (x.PrimerNombre != null && EF.Functions.Like(x.PrimerNombre, $"%{busqueda}%")));
            }

            // Traer a memoria — se evita window functions de MySQL 5.7
            var rawAlumnos = await alumnosBaseQuery.ToListAsync(cancellationToken);

            // Una fila por (alumno, carrera) — matrícula más reciente
            var listaEstudiantes = rawAlumnos
                .OrderByDescending(x => x.FechaMatricula)
                .GroupBy(x => new { x.IdAlumno, x.IdCarrera })
                .Select(g => g.First())
                .ToList();

            if (listaEstudiantes.Count == 0)
            {
                return new List<EstudiantePendienteEgresoDto>();
            }

            var idsAlumnos = listaEstudiantes.Select(x => x.IdAlumno).Distinct().ToList();
            var idsCarreras = listaEstudiantes.Select(x => x.IdCarrera).Distinct().ToList();

            // 3. Mallas — cacheadas por carreras consultadas
            string mallasCacheKey = $"mallas_carreras_{string.Join("_", idsCarreras.OrderBy(x => x))}";
            var mallasCarreras = await _cache.GetOrCreateAsync(mallasCacheKey, async mEntry =>
            {
                mEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
                return await _context.Mallas
                    .AsNoTracking()
                    .Where(m => idsCarreras.Contains(m.IdCarrera))
                    .Select(m => new { m.IdMalla, m.IdCarrera, Descripcion = m.Descripcion ?? $"Malla {m.IdMalla}", m.Activa })
                    .ToListAsync(cancellationToken);
            });

            var idsMallas = mallasCarreras!.Select(m => m.IdMalla).Distinct().ToList();

            string detalleMallasCacheKey = $"detalle_mallas_{string.Join("_", idsMallas.OrderBy(x => x))}";
            var rawDetalleMallas = await _cache.GetOrCreateAsync(detalleMallasCacheKey, async dmEntry =>
            {
                dmEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
                return await _context.Detallemallas
                    .AsNoTracking()
                    .Where(dm => idsMallas.Contains(dm.IdMalla) && (dm.Anulada == null || dm.Anulada == false))
                    .Select(dm => new { dm.IdMalla, dm.IdNivel, dm.IdAsignatura })
                    .ToListAsync(cancellationToken);
            });

            var infoMallas = mallasCarreras!.Select(m =>
            {
                var dms = rawDetalleMallas!.Where(d => d.IdMalla == m.IdMalla).ToList();
                return new
                {
                    m.IdMalla,
                    m.IdCarrera,
                    m.Descripcion,
                    m.Activa,
                    TotalNiveles = dms.Select(d => d.IdNivel).Distinct().Count(),
                    TotalMaterias = dms.Select(d => d.IdAsignatura).Distinct().Count(),
                    AsignaturasIds = dms.Select(d => d.IdAsignatura).Distinct().ToHashSet(),
                    NivelesIds = dms.Select(d => d.IdNivel).Distinct().ToHashSet()
                };
            }).ToList();

            // 4. Egresados formales y postulaciones — acotados a los candidatos del filtro
            var egresadosTitulos = (await _context.AlumnosTitulos
                .AsNoTracking()
                .Where(at => idsAlumnos.Contains(at.IdAlumno))
                .Select(at => at.IdAlumno)
                .Distinct()
                .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var postulacionesMap = (await _context.TitulPostulacionAlumnos
                .AsNoTracking()
                .Include(p => p.IdPostulacionEstadoNavigation)
                .Include(p => p.IdMatriculaNavigation)
                .Where(p => idsAlumnos.Contains(p.IdMatriculaNavigation.IdAlumno))
                .Select(p => new { p.IdMatriculaNavigation.IdAlumno, Estado = p.IdPostulacionEstadoNavigation.Nombre })
                .ToListAsync(cancellationToken))
                .GroupBy(x => x.IdAlumno)
                .ToDictionary(g => g.Key, g => g.First().Estado, StringComparer.OrdinalIgnoreCase);

            // 5. Calificaciones — acotadas a los alumnos candidatos
            var calificacionesEstudiantes = await (
                from cal in _context.Calificaciones.AsNoTracking()
                join m in _context.Matriculas.AsNoTracking() on cal.IdMatricula equals m.IdMatricula
                join p in _context.Periodos.AsNoTracking() on m.IdPeriodo equals p.IdPeriodo into pGroup
                from p in pGroup.DefaultIfEmpty()
                where idsAlumnos.Contains(m.IdAlumno)
                select new
                {
                    m.IdAlumno,
                    cal.IdMatricula,
                    cal.IdAsignatura,
                    cal.IdNivel,
                    cal.NotaFinal,
                    cal.PromedioFinal,
                    Aprobado = cal.Aprobado == true,
                    m.FechaMatricula,
                    FechaInicialPeriodo = p != null ? p.FechaInicial : null,
                    FechaFinalPeriodo = p != null ? p.FechaFinal : null
                }
            ).ToListAsync(cancellationToken);

            var califsPorAlumno = calificacionesEstudiantes
                .GroupBy(c => c.IdAlumno, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 6. Calcular avance por alumno y aplicar regla de egreso
            var listRes = new List<EstudiantePendienteEgresoDto>();

            foreach (var est in listaEstudiantes)
            {
                var mallasDeCarrera = infoMallas.Where(m => m.IdCarrera == est.IdCarrera).ToList();
                califsPorAlumno.TryGetValue(est.IdAlumno, out var alumnoCalifs);
                alumnoCalifs ??= new();

                var califsAlumno = alumnoCalifs
                    .GroupBy(c => c.IdAsignatura)
                    .Select(g => g
                        .OrderByDescending(c => c.FechaInicialPeriodo)
                        .ThenByDescending(c => c.FechaFinalPeriodo)
                        .ThenByDescending(c => c.FechaMatricula)
                        .ThenByDescending(c => c.IdMatricula)
                        .First())
                    .ToList();

                var mejorMalla = mallasDeCarrera
                    .OrderByDescending(m => califsAlumno.Count(c => c.Aprobado && m.AsignaturasIds.Contains(c.IdAsignatura)))
                    .ThenByDescending(m => califsAlumno.Count(c => m.AsignaturasIds.Contains(c.IdAsignatura)))
                    .ThenByDescending(m => m.Activa == true)
                    .FirstOrDefault();

                if (mejorMalla == null)
                {
                    continue;
                }

                var califsEnMalla = califsAlumno.Where(c => mejorMalla.AsignaturasIds.Contains(c.IdAsignatura)).ToList();

                int materiasAprobadas = califsEnMalla.Where(c => c.Aprobado).Select(c => c.IdAsignatura).Distinct().Count();
                int nivelesAprobados = califsEnMalla
                    .Where(c => c.Aprobado && c.IdNivel.HasValue && mejorMalla.NivelesIds.Contains(c.IdNivel.Value))
                    .Select(c => c.IdNivel!.Value).Distinct().Count();

                var notasValidas = califsEnMalla
                    .Select(c => c.NotaFinal is > 0 ? c.NotaFinal.Value
                               : c.PromedioFinal is > 0 ? c.PromedioFinal.Value
                               : (decimal?)null)
                    .Where(n => n.HasValue).Select(n => n!.Value).ToList();

                decimal promedio = notasValidas.Count > 0 ? Math.Round(notasValidas.Average(), 2) : 0;

                bool esTitulado = egresadosTitulos.Contains(est.IdAlumno);
                bool tienePostulacion = postulacionesMap.TryGetValue(est.IdAlumno, out var estadoPostulacion);

                bool cumplioMalla = materiasAprobadas >= mejorMalla.TotalMaterias ||
                                    (mejorMalla.TotalNiveles > 0 && nivelesAprobados >= mejorMalla.TotalNiveles);
                bool debeMostrar = esBusquedaPuntual || (cumplioMalla && !esTitulado);

                if (!debeMostrar)
                {
                    continue;
                }

                listRes.Add(new EstudiantePendienteEgresoDto(
                    est.IdAlumno,
                    $"{est.PrimerNombre} {est.SegundoNombre}".Trim(),
                    $"{est.ApellidoPaterno} {est.ApellidoMaterno}".Trim(),
                    $"{est.ApellidoPaterno} {est.ApellidoMaterno} {est.PrimerNombre} {est.SegundoNombre}".Trim(),
                    est.Email, est.Celular,
                    est.IdCarrera, est.Carrera ?? "SIN CARRERA",
                    mejorMalla.IdMalla, mejorMalla.Descripcion,
                    est.IdModalidad, est.IdModalidad != 0 ? $"Modalidad {est.IdModalidad}" : "Presencial",
                    est.IdPeriodo ?? periodoFiltro ?? "N/A",
                    nivelesAprobados, mejorMalla.TotalNiveles,
                    materiasAprobadas, mejorMalla.TotalMaterias,
                    promedio, esTitulado, tienePostulacion, estadoPostulacion
                ));
            }

            return listRes;
        });

        // 7. Paginación sobre resultados filtrados
        int totalRegistros = resultados?.Count ?? 0;
        int pagina = Math.Max(1, filtro.Pagina);
        int tamanoPagina = Math.Clamp(filtro.TamanoPagina, 1, 100);
        int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);

        var itemsPaginados = (resultados ?? new List<EstudiantePendienteEgresoDto>())
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        // 8. Enriquecimiento financiero — SOLO para los registros de la página actual con caché por alumno
        if (itemsPaginados.Count > 0)
        {
            var studentCarreraPairs = itemsPaginados
                .Select(r => new { r.IdAlumno, r.IdCarrera }).Distinct().ToList();

            var uncachedPairs = studentCarreraPairs
                .Where(sc => !_cache.TryGetValue($"finanzas_{sc.IdAlumno}_{sc.IdCarrera}", out decimal _))
                .ToList();

            if (uncachedPairs.Count > 0)
            {
                var studentIds = uncachedPairs.Select(r => r.IdAlumno).Distinct().ToList();

                var matriculasEstudiantes = await _context.Matriculas
                    .AsNoTracking()
                    .Where(m => studentIds.Contains(m.IdAlumno))
                    .Select(m => new { m.IdAlumno, m.IdMatricula, IdCarrera = m.IdNivelNavigation != null ? m.IdNivelNavigation.IdCarrera : 0 })
                    .ToListAsync(cancellationToken);

                var validMatriculas = matriculasEstudiantes
                    .Where(m => uncachedPairs.Any(sc => sc.IdAlumno == m.IdAlumno && (sc.IdCarrera == 0 || sc.IdCarrera == m.IdCarrera)))
                    .ToList();

                var matriculaIdToAlumnoKey = validMatriculas.ToDictionary(m => m.IdMatricula, m => $"{m.IdAlumno}_{m.IdCarrera}");
                var allMatriculaIds = validMatriculas.Select(m => m.IdMatricula).ToList();

                var creditosAlumno = await _context.CreditoAlumno
                    .AsNoTracking()
                    .Where(c => allMatriculaIds.Contains(c.IdMatricula) && c.Saldo > 0)
                    .Select(c => new { c.IdCredito, c.IdMatricula, c.IdEspecie, c.Saldo, c.CreditoInicial })
                    .ToListAsync(cancellationToken);

                var creditosIdsBatch = creditosAlumno.Select(c => c.IdCredito).ToList();
                var especiesIdsBatch = creditosAlumno.Select(c => c.IdEspecie).Distinct().ToList();
                var especiesDictBatch = await _context.Especies
                    .AsNoTracking()
                    .Where(e => especiesIdsBatch.Contains(e.IdEspecie))
                    .ToDictionaryAsync(e => e.IdEspecie, cancellationToken);

                var pagosCajaBatch = creditosIdsBatch.Count > 0 || allMatriculaIds.Count > 0 ? await (
                    from dp in _context.DetallePagos.AsNoTracking()
                    join p in _context.Pagos.AsNoTracking() on dp.IdPago equals p.IdPago
                    where (p.Anulado == null || p.Anulado == false) &&
                          ((dp.IdCredito != null && creditosIdsBatch.Contains(dp.IdCredito.Value)) ||
                           (p.IdMatricula != null && allMatriculaIds.Contains(p.IdMatricula.Value)))
                    select new { dp.IdCredito, p.IdMatricula, dp.IdEspecie, dp.Valor }
                ).ToListAsync(cancellationToken) : new();

                var pagosDict = pagosCajaBatch
                    .GroupBy(p => p.IdCredito.HasValue ? $"C_{p.IdCredito.Value}" : $"M_{p.IdMatricula}_{p.IdEspecie}")
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Valor ?? 0));

                var deudasCalculadas = creditosAlumno
                    .GroupBy(s => matriculaIdToAlumnoKey[s.IdMatricula])
                    .ToDictionary(g => g.Key, g =>
                    {
                        decimal totalDeudaCarrera = 0;
                        foreach (var matGrupo in g.GroupBy(x => x.IdMatricula))
                        {
                            var rubrosPorCategoria = matGrupo
                                .GroupBy(x =>
                                {
                                    especiesDictBatch.TryGetValue(x.IdEspecie, out var esp);
                                    if (esp != null && !string.IsNullOrWhiteSpace(esp.CodigoReferencia))
                                    {
                                        return esp.CodigoReferencia.Trim().ToUpper();
                                    }

                                    if (esp != null && !string.IsNullOrWhiteSpace(esp.Especie))
                                    {
                                        return esp.Especie.Trim().ToUpper();
                                    }

                                    return x.IdEspecie.ToString();
                                })
                                .Select(catGroup =>
                                {
                                    var primerItem = catGroup
                                        .OrderByDescending(x => pagosDict.GetValueOrDefault($"C_{x.IdCredito}", 0))
                                        .ThenBy(x => x.IdCredito).First();
                                    decimal costoUnico = primerItem.CreditoInicial is > 0 ? primerItem.CreditoInicial.Value : 0;
                                    decimal saldoRegUnico = costoUnico > 0 ? Math.Min(primerItem.Saldo ?? 0, costoUnico) : (primerItem.Saldo ?? 0);
                                    decimal pagadoCat = catGroup.Sum(x =>
                                    {
                                        decimal pag = pagosDict.GetValueOrDefault($"C_{x.IdCredito}", 0);
                                        if (pag == 0)
                                        {
                                            pag = pagosDict.GetValueOrDefault($"M_{x.IdMatricula}_{x.IdEspecie}", 0);
                                        }

                                        return pag;
                                    });
                                    return new { Costo = costoUnico, SaldoReg = saldoRegUnico, Pagado = pagadoCat };
                                }).ToList();

                            decimal costoSemestre = rubrosPorCategoria.Sum(x => x.Costo);
                            decimal saldoRegistradoSemestre = rubrosPorCategoria.Sum(x => x.SaldoReg);
                            decimal pagadoSemestre = rubrosPorCategoria.Sum(x => x.Pagado);
                            decimal saldoSemestre = pagadoSemestre > 0
                                ? Math.Min(saldoRegistradoSemestre, Math.Max(0, costoSemestre - pagadoSemestre))
                                : saldoRegistradoSemestre;
                            totalDeudaCarrera += saldoSemestre;
                        }
                        return totalDeudaCarrera;
                    });

                foreach (var pair in uncachedPairs)
                {
                    decimal saldo = deudasCalculadas.GetValueOrDefault($"{pair.IdAlumno}_{pair.IdCarrera}", 0);
                    _cache.Set($"finanzas_{pair.IdAlumno}_{pair.IdCarrera}", saldo, TimeSpan.FromMinutes(5));
                }
            }

            itemsPaginados = itemsPaginados.Select(est =>
            {
                decimal saldo = _cache.TryGetValue($"finanzas_{est.IdAlumno}_{est.IdCarrera}", out decimal s) ? s : 0;
                return est with { NoAdeuda = saldo <= 0, SaldoPendiente = saldo, TieneRestricciones = false };
            }).ToList();
        }

        return new PagedResultDto<EstudiantePendienteEgresoDto>(
            itemsPaginados, totalRegistros, pagina, tamanoPagina, totalPaginas);
    }

    public async Task<ExpedienteAcademicoDto?> GetExpedienteAcademicoAsync(
        string idAlumno,
        int? idCarrera = null,
        CancellationToken cancellationToken = default)
    {
        string cedula = idAlumno.Trim();

        var alumno = await _context.Alumnos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdAlumno == cedula, cancellationToken);

        if (alumno == null)
        {
            return null;
        }

        // 1. Obtener todas las matrículas del estudiante de Instituto ordenadas por nivel y fecha
        var rawMatriculas = await (
            from mat in _context.Matriculas.AsNoTracking()
            join cur in _context.Cursos.AsNoTracking() on mat.IdNivel equals cur.IdNivel into curGroup
            from cur in curGroup.DefaultIfEmpty()
            join c in _context.Carreras.AsNoTracking() on cur.IdCarrera equals c.IdCarrera into cGroup
            from c in cGroup.DefaultIfEmpty()
            join p in _context.Periodos.AsNoTracking() on mat.IdPeriodo equals p.IdPeriodo into pGroup
            from p in pGroup.DefaultIfEmpty()
            where mat.IdAlumno == cedula
               && (mat.Retirado == null || mat.Retirado == false)
               && (c == null || c.EsInstituto == null || c.EsInstituto == true)
               && (p == null || p.EsConduccion == null || p.EsConduccion == false)
            orderby cur.Orden, mat.FechaMatricula
            select new
            {
                mat.IdMatricula,
                mat.IdPeriodo,
                PeriodoDetalle = p != null ? p.Detalle : null,
                mat.IdNivel,
                NombreNivel = cur != null ? cur.Nivel : null,
                Orden = cur != null ? cur.Orden : 0,
                mat.Paralelo,
                mat.FechaMatricula,
                mat.IdModalidad,
                IdCarrera = cur != null ? cur.IdCarrera : 0,
                Carrera = c != null ? c.Carrera : null
            }
        ).ToListAsync(cancellationToken);

        var carreraReciente = rawMatriculas
            .Where(m => !idCarrera.HasValue || m.IdCarrera == idCarrera.Value)
            .OrderByDescending(m => m.FechaMatricula)
            .FirstOrDefault();

        int carreraId = carreraReciente?.IdCarrera ?? (idCarrera ?? 0);
        string nombreCarrera = carreraReciente?.Carrera ?? "SIN ASIGNAR";
        int? modalidadId = carreraReciente?.IdModalidad ?? alumno.IdModalidad;

        var matriculasCarrera = carreraId > 0
            ? rawMatriculas.Where(m => m.IdCarrera == carreraId).ToList()
            : rawMatriculas;

        var idsMatriculasCarrera = matriculasCarrera.Select(m => m.IdMatricula).ToHashSet();

        // 2. Obtener todas las calificaciones e historial de asignaturas del estudiante de esa carrera
        var rawCalificaciones = await (
            from cal in _context.Calificaciones.AsNoTracking()
            join m in _context.Matriculas.AsNoTracking() on cal.IdMatricula equals m.IdMatricula
            join cur in _context.Cursos.AsNoTracking() on cal.IdNivel equals cur.IdNivel into curGroup
            from cur in curGroup.DefaultIfEmpty()
            join asig in _context.Asignaturas.AsNoTracking() on cal.IdAsignatura equals asig.IdAsignatura into asigGroup
            from asig in asigGroup.DefaultIfEmpty()
            join p in _context.Periodos.AsNoTracking() on m.IdPeriodo equals p.IdPeriodo into pGroup
            from p in pGroup.DefaultIfEmpty()
            where m.IdAlumno == cedula && (idsMatriculasCarrera.Count == 0 || idsMatriculasCarrera.Contains(m.IdMatricula))
            select new
            {
                cal.IdMatricula,
                cal.IdAsignatura,
                NombreAsignatura = asig != null ? asig.Asignatura : null,
                cal.IdNivel,
                NombreNivel = cur != null ? cur.Nivel : null,
                OrdenNivel = cur != null ? cur.Orden : 0,
                m.IdPeriodo,
                m.FechaMatricula,
                FechaInicialPeriodo = p != null ? p.FechaInicial : null,
                FechaFinalPeriodo = p != null ? p.FechaFinal : null,
                cal.Ef1,
                cal.Ep1,
                cal.Nota1,
                cal.Ef2,
                cal.Ep2,
                cal.Nota2,
                cal.Examen,
                cal.PromedioFinal,
                cal.NotaFinal,
                Aprobado = cal.Aprobado == true,
                cal.Observacion
            }
        ).ToListAsync(cancellationToken);

        // Si la misma materia fue matriculada/cursada en varios periodos (por arrastre, repetición o duplicidad),
        // se conserva únicamente el registro correspondiente al periodo más reciente (último periodo).
        var calificacionesRecientes = rawCalificaciones
            .GroupBy(c => c.IdAsignatura > 0 ? c.IdAsignatura.ToString() : (c.NombreAsignatura?.Trim().ToUpperInvariant() ?? ""))
            .Select(g => g
                .OrderByDescending(c => c.FechaInicialPeriodo)
                .ThenByDescending(c => c.FechaFinalPeriodo)
                .ThenByDescending(c => c.FechaMatricula)
                .ThenByDescending(c => c.IdMatricula)
                .First())
            .ToList();

        // 3. Obtener mallas disponibles de la carrera y seleccionar la que mejor corresponde
        var mallasCarrera = await _context.Mallas
            .AsNoTracking()
            .Where(m => m.IdCarrera == carreraId)
            .Select(m => new
            {
                m.IdMalla,
                m.Descripcion,
                m.Activa
            })
            .ToListAsync(cancellationToken);

        var idsMallas = mallasCarrera.Select(m => m.IdMalla).Distinct().ToList();

        var detalleMallas = await (
            from dm in _context.Detallemallas.AsNoTracking()
            join asig in _context.Asignaturas.AsNoTracking() on dm.IdAsignatura equals asig.IdAsignatura into asigGroup
            from asig in asigGroup.DefaultIfEmpty()
            join cur in _context.Cursos.AsNoTracking() on dm.IdNivel equals cur.IdNivel into curGroup
            from cur in curGroup.DefaultIfEmpty()
            where idsMallas.Contains(dm.IdMalla) && (dm.Anulada == null || dm.Anulada == false)
            select new
            {
                dm.IdMalla,
                dm.IdNivel,
                NombreNivel = cur != null ? cur.Nivel : null,
                OrdenNivel = cur != null ? cur.Orden : 0,
                dm.IdAsignatura,
                NombreAsignatura = asig != null ? asig.Asignatura : null,
                dm.Creditos,
                dm.Horas
            }
        ).ToListAsync(cancellationToken);

        var infoMallas = mallasCarrera.Select(m =>
        {
            var dms = detalleMallas.Where(d => d.IdMalla == m.IdMalla).ToList();
            return new
            {
                m.IdMalla,
                Descripcion = m.Descripcion ?? $"Malla {m.IdMalla}",
                m.Activa,
                TotalNiveles = dms.Select(d => d.IdNivel).Distinct().Count(),
                TotalMaterias = dms.Select(d => d.IdAsignatura).Distinct().Count(),
                AsignaturasIds = dms.Select(d => d.IdAsignatura).Distinct().ToHashSet(),
                NivelesIds = dms.Select(d => d.IdNivel).Distinct().ToHashSet()
            };
        }).ToList();

        var mejorMalla = infoMallas
            .OrderByDescending(m => calificacionesRecientes.Count(c => c.Aprobado && m.AsignaturasIds.Contains(c.IdAsignatura)))
            .ThenByDescending(m => calificacionesRecientes.Count(c => m.AsignaturasIds.Contains(c.IdAsignatura)))
            .ThenByDescending(m => m.Activa == true)
            .FirstOrDefault();

        int mallaId = mejorMalla?.IdMalla ?? 0;
        string nombreMalla = mejorMalla?.Descripcion ?? "MALLA VIGENTE";
        int totalNivelesMalla = mejorMalla?.TotalNiveles ?? 0;
        int totalMateriasMalla = mejorMalla?.TotalMaterias ?? 0;

        var detalleMallaMap = detalleMallas
            .Where(dm => dm.IdMalla == mallaId)
            .GroupBy(dm => dm.IdAsignatura)
            .ToDictionary(g => g.Key, g => g.First());

        // 4. Lista de periodos cursados (matrículas del alumno ordenadas por nivel y fecha para esta carrera)
        var matriculas = matriculasCarrera
            .Select(m => new ExpedientePeriodoCursadoDto(
                m.IdPeriodo ?? "",
                m.PeriodoDetalle ?? m.IdPeriodo ?? "",
                m.IdNivel,
                m.NombreNivel ?? $"Nivel {m.IdNivel}",
                m.Orden,
                m.Paralelo,
                m.FechaMatricula
            ))
            .ToList();

        // 5. Historial de calificaciones y asignaturas ordenadas por nivel y nombre (estructurado según la malla oficial)
        var listaAsignaturas = new List<(int OrdenNivel, ExpedienteAsignaturaDto Dto)>();

        if (mejorMalla != null)
        {
            var materiasDeMalla = detalleMallas
                .Where(dm => dm.IdMalla == mallaId)
                .GroupBy(dm => dm.IdAsignatura)
                .Select(g => g.First())
                .ToList();

            foreach (var dm in materiasDeMalla)
            {
                // Buscar si el estudiante cursó esta materia por IdAsignatura o por concordancia exacta de nombre
                var cal = calificacionesRecientes.FirstOrDefault(c => c.IdAsignatura == dm.IdAsignatura);
                if (cal == null && !string.IsNullOrWhiteSpace(dm.NombreAsignatura))
                {
                    cal = calificacionesRecientes.FirstOrDefault(c =>
                        !string.IsNullOrWhiteSpace(c.NombreAsignatura) &&
                        string.Equals(c.NombreAsignatura.Trim(), dm.NombreAsignatura.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                int orden = (dm.OrdenNivel.HasValue && dm.OrdenNivel.Value > 0)
                    ? dm.OrdenNivel.Value
                    : (cal?.OrdenNivel ?? 0);

                string nombreNivel = cal?.NombreNivel ?? dm.NombreNivel ?? (dm.IdNivel > 0 ? $"Nivel {dm.IdNivel}" : "-");

                if (cal != null)
                {
                    decimal? notaEfectiva = cal.NotaFinal.HasValue && cal.NotaFinal.Value > 0
                        ? cal.NotaFinal.Value
                        : (cal.PromedioFinal.HasValue && cal.PromedioFinal.Value > 0 ? cal.PromedioFinal.Value : cal.NotaFinal ?? cal.PromedioFinal);

                    var dto = new ExpedienteAsignaturaDto(
                        dm.IdAsignatura,
                        dm.NombreAsignatura ?? cal.NombreAsignatura ?? $"Asignatura {dm.IdAsignatura}",
                        dm.IdNivel,
                        nombreNivel,
                        dm.Creditos,
                        dm.Horas,
                        cal.IdPeriodo,
                        cal.Ef1,
                        cal.Ep1,
                        cal.Nota1,
                        cal.Ef2,
                        cal.Ep2,
                        cal.Nota2,
                        cal.Examen,
                        cal.PromedioFinal,
                        notaEfectiva,
                        cal.Aprobado,
                        cal.Observacion
                    );

                    listaAsignaturas.Add((orden, dto));
                }
                else
                {
                    var dto = new ExpedienteAsignaturaDto(
                        dm.IdAsignatura,
                        dm.NombreAsignatura ?? $"Asignatura {dm.IdAsignatura}",
                        dm.IdNivel,
                        nombreNivel,
                        dm.Creditos,
                        dm.Horas,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        false,
                        "Pendiente por cursar"
                    );

                    listaAsignaturas.Add((orden, dto));
                }
            }
        }
        else
        {
            // En caso de que no exista malla definida, listar directamente las asignaturas cursadas
            foreach (var cal in calificacionesRecientes)
            {
                decimal? notaEfectiva = cal.NotaFinal.HasValue && cal.NotaFinal.Value > 0
                    ? cal.NotaFinal.Value
                    : (cal.PromedioFinal.HasValue && cal.PromedioFinal.Value > 0 ? cal.PromedioFinal.Value : cal.NotaFinal ?? cal.PromedioFinal);

                var dto = new ExpedienteAsignaturaDto(
                    cal.IdAsignatura,
                    cal.NombreAsignatura ?? $"Asignatura {cal.IdAsignatura}",
                    cal.IdNivel,
                    cal.NombreNivel ?? (cal.IdNivel.HasValue ? $"Nivel {cal.IdNivel}" : "-"),
                    null,
                    null,
                    cal.IdPeriodo,
                    cal.Ef1,
                    cal.Ep1,
                    cal.Nota1,
                    cal.Ef2,
                    cal.Ep2,
                    cal.Nota2,
                    cal.Examen,
                    cal.PromedioFinal,
                    notaEfectiva,
                    cal.Aprobado,
                    cal.Observacion
                );

                listaAsignaturas.Add((cal.OrdenNivel ?? 0, dto));
            }
        }

        var calificaciones = listaAsignaturas
            .OrderBy(x => x.OrdenNivel)
            .ThenBy(x => x.Dto.NombreNivel)
            .ThenBy(x => x.Dto.NombreAsignatura)
            .Select(x => x.Dto)
            .ToList();

        var califsMalla = mejorMalla != null
            ? calificaciones.Where(c => mejorMalla.AsignaturasIds.Contains(c.IdAsignatura)).ToList()
            : calificaciones;

        int materiasAprobadas = califsMalla.Where(c => c.Aprobado).Select(c => c.IdAsignatura).Distinct().Count();
        int nivelesAprobados = mejorMalla != null
            ? califsMalla.Where(c => c.Aprobado && c.Nivel.HasValue && mejorMalla.NivelesIds.Contains(c.Nivel.Value)).Select(c => c.Nivel!.Value).Distinct().Count()
            : califsMalla.Where(c => c.Aprobado && c.Nivel.HasValue && c.Nivel.Value != 0).Select(c => c.Nivel!.Value).Distinct().Count();

        var notasValidas = califsMalla
            .Where(c => c.NotaFinal.HasValue && c.NotaFinal.Value > 0)
            .Select(c => c.NotaFinal!.Value)
            .ToList();

        decimal promedio = notasValidas.Count > 0 ? Math.Round(notasValidas.Average(), 2) : 0;

        // 6. Información de título formal si ya egresó
        var titulo = await _context.AlumnosTitulos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdAlumno == cedula, cancellationToken);

        // 7. Información Financiera y Detalle de Pagos por Semestre
        ExpedienteFinancieroDto? financieroDto = null;
        if (idsMatriculasCarrera.Count > 0)
        {
            var creditosAlumno = await _context.CreditoAlumno
                .AsNoTracking()
                .Where(c => idsMatriculasCarrera.Contains(c.IdMatricula))
                .ToListAsync(cancellationToken);

            var especiesIds = creditosAlumno.Select(c => c.IdEspecie).Distinct().ToList();
            var especiesDict = await _context.Especies
                .AsNoTracking()
                .Where(e => especiesIds.Contains(e.IdEspecie))
                .ToDictionaryAsync(e => e.IdEspecie, cancellationToken);

            var creditosIds = creditosAlumno.Select(c => c.IdCredito).Distinct().ToList();

            var detallePagos = await (
                from dp in _context.DetallePagos.AsNoTracking()
                join p in _context.Pagos.AsNoTracking() on dp.IdPago equals p.IdPago
                where (dp.IdCredito != null && creditosIds.Contains(dp.IdCredito.Value))
                   || (p.IdMatricula != null && idsMatriculasCarrera.Contains(p.IdMatricula.Value) && especiesIds.Contains(dp.IdEspecie))
                select new
                {
                    dp.IdPago,
                    dp.IdCredito,
                    dp.IdEspecie,
                    p.IdMatricula,
                    p.Fecha,
                    p.Factura,
                    p.NumeroDeposito,
                    p.Anulado,
                    dp.Valor,
                    dp.Descuento
                }
            ).ToListAsync(cancellationToken);

            var pagosValidos = detallePagos
                .Where(dp => dp.Anulado == null || dp.Anulado == false)
                .ToList();

            var semestresList = new List<ExpedienteSemestreFinancieroDto>();

            foreach (var mat in matriculasCarrera.OrderBy(m => m.Orden).ThenBy(m => m.FechaMatricula))
            {
                var creditosMat = creditosAlumno.Where(c => c.IdMatricula == mat.IdMatricula).ToList();

                var tempRubros = new List<(CreditoAlumno cred, Especies? esp, decimal valorInicial, decimal saldoRegistrado, List<ExpedientePagoRealizadoDto> pagosRubro, decimal totalPagadoCaja)>();

                foreach (var cred in creditosMat)
                {
                    especiesDict.TryGetValue(cred.IdEspecie, out var esp);
                    decimal valorOficial = esp?.Valor ?? 0;
                    decimal valorInicial = cred.CreditoInicial.HasValue && cred.CreditoInicial.Value > 0
                        ? cred.CreditoInicial.Value
                        : valorOficial;

                    var pagosRubro = pagosValidos
                        .Where(dp => (dp.IdCredito != null && dp.IdCredito.Value == cred.IdCredito)
                                  || (dp.IdMatricula != null && dp.IdMatricula.Value == mat.IdMatricula && dp.IdEspecie == cred.IdEspecie))
                        .Select(p => new ExpedientePagoRealizadoDto(
                            p.IdPago,
                            p.Fecha,
                            p.Factura,
                            p.NumeroDeposito,
                            p.Valor ?? 0,
                            p.Descuento
                        ))
                        .DistinctBy(p => p.IdPago)
                        .OrderBy(p => p.FechaPago)
                        .ToList();

                    decimal totalPagadoCaja = pagosRubro.Sum(p => p.ValorPagado);

                    decimal saldoRegistrado = cred.CreditoInicial.HasValue && cred.CreditoInicial.Value > 0
                        ? Math.Min(cred.Saldo ?? 0, cred.CreditoInicial.Value)
                        : (cred.Saldo ?? 0);

                    tempRubros.Add((cred, esp, valorInicial, saldoRegistrado, pagosRubro, totalPagadoCaja));
                }

                // Excedente de dinero entre rubros del mismo semestre (ej. si colecturía registró pago de colegiatura bajo la especie matrícula)
                decimal excedenteDisponible = tempRubros
                    .Where(t => t.totalPagadoCaja > t.valorInicial)
                    .Sum(t => t.totalPagadoCaja - t.valorInicial);

                // Agrupar por categoría (Ref o Nombre) para detectar duplicados (ej. 2 colegiaturas cargadas en la misma matrícula)
                var rubrosCategorizados = tempRubros
                    .GroupBy(t =>
                    {
                        if (t.esp != null && !string.IsNullOrWhiteSpace(t.esp.CodigoReferencia))
                        {
                            return t.esp.CodigoReferencia.Trim().ToUpper();
                        }

                        if (t.esp != null && !string.IsNullOrWhiteSpace(t.esp.Especie))
                        {
                            return t.esp.Especie.Trim().ToUpper();
                        }

                        return t.cred.IdEspecie.ToString();
                    })
                    .ToList();

                var rubrosList = new List<ExpedienteRubroFinancieroDto>();

                foreach (var catGroup in rubrosCategorizados)
                {
                    var itemsEnCat = catGroup
                        .OrderByDescending(t => t.totalPagadoCaja)
                        .ThenBy(t => t.cred.IdCredito)
                        .ToList();

                    // El primero es el rubro principal legítimo
                    var principal = itemsEnCat[0];
                    decimal totalPagadoEnCat = itemsEnCat.Sum(t => t.totalPagadoCaja);
                    var todosLosPagosCat = itemsEnCat.SelectMany(t => t.pagosRubro).DistinctBy(p => p.IdPago).OrderBy(p => p.FechaPago).ToList();

                    decimal abonoEfectivo = totalPagadoEnCat;
                    if (abonoEfectivo < principal.valorInicial && excedenteDisponible > 0)
                    {
                        decimal falta = principal.valorInicial - abonoEfectivo;
                        decimal aplicar = Math.Min(falta, excedenteDisponible);
                        abonoEfectivo += aplicar;
                        excedenteDisponible -= aplicar;
                    }

                    decimal saldoPendiente = abonoEfectivo > 0
                        ? Math.Min(principal.saldoRegistrado, Math.Max(0, principal.valorInicial - abonoEfectivo))
                        : principal.saldoRegistrado;

                    decimal totalAbonado = Math.Min(principal.valorInicial, Math.Max(abonoEfectivo, principal.valorInicial - saldoPendiente));

                    string estadoRubro = saldoPendiente <= 0
                        ? "PAGADO"
                        : (totalAbonado > 0 ? "ABONO PARCIAL" : "PENDIENTE");

                    rubrosList.Add(new ExpedienteRubroFinancieroDto(
                        principal.cred.IdCredito,
                        principal.cred.IdEspecie,
                        principal.esp?.Especie ?? $"Especie {principal.cred.IdEspecie}",
                        principal.esp?.CodigoReferencia,
                        principal.valorInicial,
                        totalAbonado,
                        saldoPendiente,
                        estadoRubro,
                        todosLosPagosCat
                    ));
                }

                decimal totalSemestre = rubrosList.Sum(r => r.ValorInicial);
                decimal totalAbonadoSemestre = rubrosList.Sum(r => r.TotalAbonado);
                decimal saldoPendienteSemestre = rubrosList.Sum(r => r.SaldoPendiente);
                string estadoSemestre = saldoPendienteSemestre <= 0
                    ? "AL DIA"
                    : (totalAbonadoSemestre > 0 ? "ABONO PARCIAL" : "PENDIENTE");

                semestresList.Add(new ExpedienteSemestreFinancieroDto(
                    mat.IdMatricula,
                    mat.IdPeriodo ?? "",
                    mat.PeriodoDetalle ?? mat.IdPeriodo ?? "",
                    mat.IdNivel,
                    mat.NombreNivel ?? $"Nivel {mat.IdNivel}",
                    mat.FechaMatricula,
                    totalSemestre,
                    totalAbonadoSemestre,
                    saldoPendienteSemestre,
                    estadoSemestre,
                    rubrosList
                ));
            }

            decimal saldoTotalPendiente = semestresList.Sum(s => s.SaldoPendiente);
            decimal totalCreditoCargado = semestresList.Sum(s => s.TotalSemestre);
            decimal totalAbonadoGlobal = semestresList.Sum(s => s.TotalAbonado);
            bool estaAlDia = saldoTotalPendiente <= 0;

            financieroDto = new ExpedienteFinancieroDto(
                saldoTotalPendiente,
                totalCreditoCargado,
                totalAbonadoGlobal,
                estaAlDia,
                semestresList
            );
        }

        string nombreCompleto = $"{alumno.ApellidoPaterno} {alumno.ApellidoMaterno} {alumno.PrimerNombre} {alumno.SegundoNombre}".Trim();

        return new ExpedienteAcademicoDto(
            alumno.IdAlumno,
            $"{alumno.PrimerNombre} {alumno.SegundoNombre}".Trim(),
            $"{alumno.ApellidoPaterno} {alumno.ApellidoMaterno}".Trim(),
            nombreCompleto,
            alumno.Email,
            alumno.EmailInstitucional,
            alumno.Celular,
            alumno.Telefono,
            alumno.Direccion,
            alumno.CiudadResidencia,
            carreraId,
            nombreCarrera,
            mallaId,
            nombreMalla,
            modalidadId,
            modalidadId.HasValue && modalidadId.Value != 0 ? $"Modalidad {modalidadId.Value}" : "Presencial",
            nivelesAprobados,
            totalNivelesMalla,
            materiasAprobadas,
            totalMateriasMalla,
            promedio,
            titulo != null,
            titulo?.NumeroActa,
            titulo?.FechaActa,
            matriculas,
            calificaciones,
            financieroDto
        );
    }
}
