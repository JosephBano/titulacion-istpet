using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.ConfiguracionGeneral;
using TitulacionIstpet.Application.Features.ConfiguracionGeneral.DTOs;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public sealed class RepositorioConfiguracionGeneral(SigafiDbContext context) : IRepositorioConfiguracionGeneral
{
    private readonly SigafiDbContext _context = context;

    #region Modalidades
    public async Task<IReadOnlyList<ModalidadMaestraDto>> ListarModalidadesAsync(bool soloActivas = false, CancellationToken ct = default)
    {
        var query = _context.TitulModalidades
            .AsNoTracking()
            .Include(m => m.TitulRequisitoModalidad)
            .AsQueryable();

        if (soloActivas)
        {
            query = query.Where(m => m.EsActivo == true);
        }

        return await query
            .OrderBy(m => m.ModalidadTitulacion)
            .Select(m => new ModalidadMaestraDto(
                m.IdModalidadTitulacion,
                m.ModalidadTitulacion ?? string.Empty,
                m.EsComplexivo,
                m.EsArticuloCientifico,
                m.GeneraTesis,
                m.CantidadMinima,
                m.EsActivo ?? false,
                m.TitulRequisitoModalidad.Count(r => r.EsActivo)
            ))
            .ToListAsync(ct);
    }

    public async Task<ModalidadMaestraDto?> ObtenerModalidadPorIdAsync(int idModalidad, CancellationToken ct = default)
    {
        var m = await _context.TitulModalidades
            .AsNoTracking()
            .Include(m => m.TitulRequisitoModalidad)
            .FirstOrDefaultAsync(m => m.IdModalidadTitulacion == idModalidad, ct);

        if (m == null)
        {
            return null;
        }

        return new ModalidadMaestraDto(
            m.IdModalidadTitulacion,
            m.ModalidadTitulacion ?? string.Empty,
            m.EsComplexivo,
            m.EsArticuloCientifico,
            m.GeneraTesis,
            m.CantidadMinima,
            m.EsActivo ?? false,
            m.TitulRequisitoModalidad.Count(r => r.EsActivo)
        );
    }

    public async Task<int> CrearModalidadAsync(CrearModalidadMaestraDto dto, CancellationToken ct = default)
    {
        var modalidadNombre = dto.ModalidadTitulacion.Trim();
        var entidad = new TitulModalidades
        {
            ModalidadTitulacion = modalidadNombre.Length > 45 ? modalidadNombre[..45] : modalidadNombre,
            EsComplexivo = dto.EsComplexivo ?? "NO",
            EsArticuloCientifico = dto.EsArticuloCientifico ?? "NO",
            GeneraTesis = dto.GeneraTesis ?? "NO",
            CantidadMinima = dto.CantidadMinima ?? 1,
            EsActivo = true
        };

        _context.TitulModalidades.Add(entidad);
        await _context.SaveChangesAsync(ct);
        return entidad.IdModalidadTitulacion;
    }

    public async Task ActualizarModalidadAsync(ActualizarModalidadMaestraDto dto, CancellationToken ct = default)
    {
        var m = await _context.TitulModalidades
            .FirstOrDefaultAsync(x => x.IdModalidadTitulacion == dto.IdModalidadTitulacion, ct)
            ?? throw new NoEncontradoException("Modalidad de Titulación", dto.IdModalidadTitulacion);

        m.ModalidadTitulacion = dto.ModalidadTitulacion.Trim();
        m.EsComplexivo = dto.EsComplexivo;
        m.EsArticuloCientifico = dto.EsArticuloCientifico;
        m.GeneraTesis = dto.GeneraTesis;
        m.CantidadMinima = dto.CantidadMinima;
        m.EsActivo = dto.EsActivo;

        await _context.SaveChangesAsync(ct);
    }

    public async Task CambiarEstadoModalidadAsync(int idModalidad, bool activo, CancellationToken ct = default)
    {
        var m = await _context.TitulModalidades
            .FirstOrDefaultAsync(x => x.IdModalidadTitulacion == idModalidad, ct)
            ?? throw new NoEncontradoException("Modalidad de Titulación", idModalidad);

        m.EsActivo = activo;
        await _context.SaveChangesAsync(ct);
    }
    #endregion

    #region Requisitos
    public async Task<IReadOnlyList<RequisitoMaestroDto>> ListarRequisitosAsync(bool soloActivos = false, CancellationToken ct = default)
    {
        var query = _context.TitulRequisitos.AsNoTracking().AsQueryable();

        if (soloActivos)
        {
            query = query.Where(r => r.EsActivo == true);
        }

        return await query
            .OrderBy(r => r.Requisito)
            .Select(r => new RequisitoMaestroDto(
                r.IdRequisitos,
                r.Requisito ?? string.Empty,
                r.EsAdjunto ?? false,
                r.EsBool ?? false,
                r.SubeAlumno ?? false,
                r.SubeColaborador ?? false,
                r.EsActivo ?? false
            ))
            .ToListAsync(ct);
    }

    public async Task<RequisitoMaestroDto?> ObtenerRequisitoPorIdAsync(int idRequisito, CancellationToken ct = default)
    {
        var r = await _context.TitulRequisitos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdRequisitos == idRequisito, ct);

        if (r == null)
        {
            return null;
        }

        return new RequisitoMaestroDto(
            r.IdRequisitos,
            r.Requisito ?? string.Empty,
            r.EsAdjunto ?? false,
            r.EsBool ?? false,
            r.SubeAlumno ?? false,
            r.SubeColaborador ?? false,
            r.EsActivo ?? false
        );
    }

    public async Task<int> CrearRequisitoAsync(CrearRequisitoMaestroDto dto, CancellationToken ct = default)
    {
        var entidad = new TitulRequisitos
        {
            Requisito = dto.Requisito.Trim(),
            EsAdjunto = dto.EsAdjunto,
            EsBool = dto.EsBool,
            SubeAlumno = dto.SubeAlumno,
            SubeColaborador = dto.SubeColaborador,
            EsActivo = true
        };

        _context.TitulRequisitos.Add(entidad);
        await _context.SaveChangesAsync(ct);
        return entidad.IdRequisitos;
    }

    public async Task ActualizarRequisitoAsync(ActualizarRequisitoMaestroDto dto, CancellationToken ct = default)
    {
        var r = await _context.TitulRequisitos
            .FirstOrDefaultAsync(x => x.IdRequisitos == dto.IdRequisitos, ct)
            ?? throw new NoEncontradoException("Requisito", dto.IdRequisitos);

        r.Requisito = dto.Requisito.Trim();
        r.EsAdjunto = dto.EsAdjunto;
        r.EsBool = dto.EsBool;
        r.SubeAlumno = dto.SubeAlumno;
        r.SubeColaborador = dto.SubeColaborador;
        r.EsActivo = dto.EsActivo;

        await _context.SaveChangesAsync(ct);
    }

    public async Task CambiarEstadoRequisitoAsync(int idRequisito, bool activo, CancellationToken ct = default)
    {
        var r = await _context.TitulRequisitos
            .FirstOrDefaultAsync(x => x.IdRequisitos == idRequisito, ct)
            ?? throw new NoEncontradoException("Requisito", idRequisito);

        r.EsActivo = activo;
        await _context.SaveChangesAsync(ct);
    }
    #endregion

    #region Matriz Requisito - Modalidad
    public async Task<IReadOnlyList<RequisitoModalidadMatrizDto>> ListarMatrizPorModalidadAsync(int idModalidad, CancellationToken ct = default)
    {
        return await _context.TitulRequisitoModalidad
            .AsNoTracking()
            .Include(rm => rm.IdModalidadTitulacionNavigation)
            .Include(rm => rm.IdRequisitosNavigation)
            .Where(rm => rm.IdModalidadTitulacion == idModalidad)
            .Select(rm => new RequisitoModalidadMatrizDto(
                rm.IdRequisitoModalidad,
                rm.IdModalidadTitulacion,
                rm.IdModalidadTitulacionNavigation.ModalidadTitulacion ?? string.Empty,
                rm.IdRequisitos,
                rm.IdRequisitosNavigation.Requisito ?? string.Empty,
                rm.IdRequisitosNavigation.EsAdjunto ?? false,
                rm.IdRequisitosNavigation.EsBool ?? false,
                rm.IdRequisitosNavigation.SubeAlumno ?? false,
                rm.IdRequisitosNavigation.SubeColaborador ?? false,
                rm.EsRequistoFinal ?? false,
                rm.EsActivo
            ))
            .ToListAsync(ct);
    }

    public async Task<int> AsignarRequisitoAModalidadAsync(AsignarRequisitoModalidadDto dto, CancellationToken ct = default)
    {
        var existente = await _context.TitulRequisitoModalidad
            .FirstOrDefaultAsync(rm => rm.IdModalidadTitulacion == dto.IdModalidadTitulacion && rm.IdRequisitos == dto.IdRequisitos, ct);

        if (existente != null)
        {
            existente.EsActivo = true;
            existente.EsRequistoFinal = dto.EsRequisitoFinal;
            existente.FechaRegistro = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return existente.IdRequisitoModalidad;
        }

        var entidad = new TitulRequisitoModalidad
        {
            IdModalidadTitulacion = dto.IdModalidadTitulacion,
            IdRequisitos = dto.IdRequisitos,
            EsRequistoFinal = dto.EsRequisitoFinal,
            FechaRegistro = DateTime.UtcNow,
            EsActivo = true
        };

        _context.TitulRequisitoModalidad.Add(entidad);
        await _context.SaveChangesAsync(ct);
        return entidad.IdRequisitoModalidad;
    }

    public async Task DesasignarRequisitoDeModalidadAsync(int idRequisitoModalidad, CancellationToken ct = default)
    {
        var rm = await _context.TitulRequisitoModalidad
            .FirstOrDefaultAsync(x => x.IdRequisitoModalidad == idRequisitoModalidad, ct)
            ?? throw new NoEncontradoException("Asignación de Requisito a Modalidad", idRequisitoModalidad);

        rm.EsActivo = false;
        rm.FechaDesactiva = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
    #endregion

    #region Resumen General
    public async Task<ResumenGeneralSistemaDto> ObtenerResumenGeneralAsync(CancellationToken ct = default)
    {
        var cohorte = await _context.TitulCohortes
            .AsNoTracking()
            .Include(c => c.IdPeriodoNavigation)
            .Include(c => c.TitulCohortesCarreras)
            .OrderByDescending(c => c.EsActivo)
            .ThenByDescending(c => c.IdCohorte)
            .FirstOrDefaultAsync(ct);

        var totalModalidades = await _context.TitulModalidades.CountAsync(m => m.EsActivo == true, ct);
        var totalRequisitos = await _context.TitulRequisitos.CountAsync(r => r.EsActivo == true, ct);

        var postulacionesQuery = _context.TitulPostulacionAlumnos.AsNoTracking();
        var totalPostulaciones = await postulacionesQuery.CountAsync(ct);
        var totalAprobadas = await postulacionesQuery.CountAsync(p => p.IdPostulacionEstadoNavigation.Nombre.Contains("Aprob"), ct);
        var totalEnRevision = await postulacionesQuery.CountAsync(p => p.IdPostulacionEstadoNavigation.Nombre.Contains("Revis") || p.IdPostulacionEstadoNavigation.Nombre.Contains("Registr"), ct);
        var totalObservadas = await postulacionesQuery.CountAsync(p => p.IdPostulacionEstadoNavigation.Nombre.Contains("Observ"), ct);
        var totalRechazadas = await postulacionesQuery.CountAsync(p => p.IdPostulacionEstadoNavigation.Nombre.Contains("Rechaz"), ct);

        string? periodoCodigo = cohorte?.IdPeriodo;
        string? periodoNombreHumano = cohorte?.IdPeriodoNavigation?.Detalle;
        string? convocatoriaDetalle = cohorte?.Detelle;
        DateTime? fechaInicio = cohorte?.FechaInicio;
        DateTime? fechaFin = cohorte?.FechaFin;
        bool estaVigente = cohorte != null && cohorte.EsActivo == true &&
                           (!fechaInicio.HasValue || DateTime.UtcNow >= fechaInicio.Value) &&
                           (!fechaFin.HasValue || DateTime.UtcNow <= fechaFin.Value);

        int? diasRestantes = null;
        if (fechaFin.HasValue)
        {
            var diff = (fechaFin.Value.Date - DateTime.UtcNow.Date).Days;
            diasRestantes = diff >= 0 ? diff : 0;
        }

        int totalCarreras = cohorte?.TitulCohortesCarreras.Count(cc => cc.EsActivo == true || cc.EsActivo == null) ?? 0;
        string estadoOp = estaVigente ? "CONVOCATORIA_VIGENTE" : (cohorte != null ? "CONVOCATORIA_CERRADA" : "SIN_CONVOCATORIA");

        return new ResumenGeneralSistemaDto(
            periodoCodigo,
            periodoNombreHumano,
            convocatoriaDetalle,
            fechaInicio,
            fechaFin,
            diasRestantes,
            estaVigente,
            totalCarreras,
            totalModalidades,
            totalRequisitos,
            totalPostulaciones,
            totalAprobadas,
            totalEnRevision,
            totalObservadas,
            totalRechazadas,
            estadoOp
        );
    }
    #endregion
}
