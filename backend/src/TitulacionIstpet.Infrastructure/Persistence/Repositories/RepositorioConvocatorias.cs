using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Convocatorias;
using TitulacionIstpet.Application.Features.Convocatorias.DTOs;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public sealed class RepositorioConvocatorias(SigafiDbContext context) : IRepositorioConvocatorias
{
    private readonly SigafiDbContext _context = context;

    public async Task<int> AperturarPeriodoConvocatoriaAsync(
        AperturarPeriodoConvocatoriaComando comando, CancellationToken ct = default)
    {
        // 1. Desactivar cohortes anteriores si se va a activar la nueva
        var cohortesActivas = await _context.TitulCohortes
            .Where(c => c.EsActivo == true)
            .ToListAsync(ct);

        foreach (var c in cohortesActivas)
        {
            c.EsActivo = false;
        }

        // 2. Crear nueva Cohorte
        var cohorte = new TitulCohortes
        {
            IdPeriodo = comando.IdPeriodo.Trim(),
            Detelle = comando.DetalleConvocatoria.Trim().Length > 45 
                ? comando.DetalleConvocatoria.Trim()[..45] 
                : comando.DetalleConvocatoria.Trim(),
            FechaInicio = comando.FechaInicioCorte,
            FechaFin = comando.FechaFinCorte,
            DiasPermitidos = comando.DiasPermitidos,
            DiasExtension = comando.DiasExtension,
            EsActivo = true
        };

        _context.TitulCohortes.Add(cohorte);
        await _context.SaveChangesAsync(ct);

        // 3. Obtener las modalidades de carrera del instituto (todas o seleccionadas)
        var queryCarreras = _context.ModalidadesCarreras
            .AsNoTracking()
            .Include(mc => mc.IdCarreraNavigation)
            .Include(mc => mc.IdModalidadNavigation)
            .Where(mc => (mc.IdCarreraNavigation.Activa == true || mc.IdCarreraNavigation.Activa == null) &&
                         (mc.EsActivo == true || mc.EsActivo == null));

        if (!comando.HabilitarTodasLasCarreras)
        {
            if (comando.IdsModalidadesCarrerasHabilitadas != null && comando.IdsModalidadesCarrerasHabilitadas.Count > 0)
            {
                queryCarreras = queryCarreras.Where(mc => comando.IdsModalidadesCarrerasHabilitadas.Contains(mc.IdModalidadCarrera));
            }
            else if (comando.IdsCarrerasHabilitadas != null && comando.IdsCarrerasHabilitadas.Count > 0)
            {
                queryCarreras = queryCarreras.Where(mc => comando.IdsCarrerasHabilitadas.Contains(mc.IdCarrera));
            }
        }

        var modalidadesCarreras = await queryCarreras.ToListAsync(ct);

        // 4. Obtener las modalidades de titulación maestras a habilitar
        var queryModalidades = _context.TitulModalidades
            .AsNoTracking()
            .Where(m => m.EsActivo == true);

        if (comando.IdsModalidadesHabilitadas != null && comando.IdsModalidadesHabilitadas.Count > 0)
        {
            queryModalidades = queryModalidades.Where(m => comando.IdsModalidadesHabilitadas.Contains(m.IdModalidadTitulacion));
        }

        var modalidadesTitulacion = await queryModalidades.ToListAsync(ct);

        // 5. Auto-vincular cada carrera y sus modalidades de titulación
        foreach (var mc in modalidadesCarreras)
        {
            var cohorteCarrera = new TitulCohortesCarreras
            {
                IdCohorte = cohorte.IdCohorte,
                IdModalidadCarrera = mc.IdModalidadCarrera,
                EsActivo = true
            };

            _context.TitulCohortesCarreras.Add(cohorteCarrera);
            await _context.SaveChangesAsync(ct);

            foreach (var mt in modalidadesTitulacion)
            {
                var mtc = new TitulModalidadesTitulacionCarreras
                {
                    IdCohorteCarrera = cohorteCarrera.IdCohorteCarrera,
                    IdModalidadTitulacion = mt.IdModalidadTitulacion,
                    FechaRegistro = DateTime.UtcNow,
                    EsActivo = true
                };

                _context.TitulModalidadesTitulacionCarreras.Add(mtc);
            }
        }

        await _context.SaveChangesAsync(ct);
        return cohorte.IdCohorte;
    }

    public async Task<ConvocatoriaDetalleDto?> ObtenerConvocatoriaActivaAsync(CancellationToken ct = default)
    {
        var cohorte = await _context.TitulCohortes
            .AsNoTracking()
            .Where(c => c.EsActivo == true)
            .OrderByDescending(c => c.IdCohorte)
            .FirstOrDefaultAsync(ct);

        if (cohorte == null)
        {
            return null;
        }

        return await MapearDetalleAsync(cohorte, ct);
    }

    public async Task<ConvocatoriaDetalleDto?> ObtenerConvocatoriaPorIdAsync(int idCohorte, CancellationToken ct = default)
    {
        var cohorte = await _context.TitulCohortes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCohorte == idCohorte, ct);

        if (cohorte == null)
        {
            return null;
        }

        return await MapearDetalleAsync(cohorte, ct);
    }

    public async Task<IReadOnlyList<ConvocatoriaResumenDto>> ListarConvocatoriasAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var cohortes = await _context.TitulCohortes
            .AsNoTracking()
            .Include(c => c.TitulCohortesCarreras)
                .ThenInclude(cc => cc.TitulModalidadesTitulacionCarreras)
                    .ThenInclude(mtc => mtc.TitulPostulacionAlumnos)
            .OrderByDescending(c => c.IdCohorte)
            .ToListAsync(ct);

        return cohortes.Select(c =>
        {
            bool vigente = c.EsActivo == true &&
                           (!c.FechaInicio.HasValue || c.FechaInicio.Value <= now) &&
                           (!c.FechaFin.HasValue || c.FechaFin.Value >= now);

            int totalCarreras = c.TitulCohortesCarreras.Count(cc => cc.EsActivo == true);
            int totalPostulaciones = c.TitulCohortesCarreras
                .SelectMany(cc => cc.TitulModalidadesTitulacionCarreras)
                .SelectMany(mtc => mtc.TitulPostulacionAlumnos)
                .Count();

            return new ConvocatoriaResumenDto(
                c.IdCohorte,
                c.IdPeriodo ?? string.Empty,
                c.Detelle ?? string.Empty,
                c.FechaInicio,
                c.FechaFin,
                c.DiasPermitidos,
                c.DiasExtension,
                c.EsActivo ?? false,
                vigente,
                totalCarreras,
                totalPostulaciones
            );
        }).ToList();
    }

    public async Task AjustarFechasCorteAsync(AjustarFechasCorteComando comando, CancellationToken ct = default)
    {
        var c = await _context.TitulCohortes
            .FirstOrDefaultAsync(x => x.IdCohorte == comando.IdCohorte, ct)
            ?? throw new NoEncontradoException("Cohorte / Convocatoria", comando.IdCohorte);

        if (comando.FechaInicio.HasValue)
        {
            c.FechaInicio = comando.FechaInicio.Value;
        }
        if (comando.FechaFin.HasValue)
        {
            c.FechaFin = comando.FechaFin.Value;
        }
        if (comando.DiasPermitidos.HasValue)
        {
            c.DiasPermitidos = comando.DiasPermitidos.Value;
        }
        if (comando.DiasExtension.HasValue)
        {
            c.DiasExtension = comando.DiasExtension.Value;
        }
        if (comando.EsActivo.HasValue)
        {
            c.EsActivo = comando.EsActivo.Value;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task ConmutarModalidadCarreraAsync(ConmutarModalidadCarreraComando comando, CancellationToken ct = default)
    {
        var mtc = await _context.TitulModalidadesTitulacionCarreras
            .FirstOrDefaultAsync(x => x.IdModalidadTitulacionCarrera == comando.IdModalidadTitulacionCarrera, ct)
            ?? throw new NoEncontradoException("Modalidad de Titulación por Carrera", comando.IdModalidadTitulacionCarrera);

        mtc.EsActivo = comando.EsActivo;
        await _context.SaveChangesAsync(ct);
    }

    private async Task<ConvocatoriaDetalleDto> MapearDetalleAsync(TitulCohortes cohorte, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        bool vigente = cohorte.EsActivo == true &&
                       (!cohorte.FechaInicio.HasValue || cohorte.FechaInicio.Value <= now) &&
                       (!cohorte.FechaFin.HasValue || cohorte.FechaFin.Value >= now);

        var carreras = await _context.TitulCohortesCarreras
            .AsNoTracking()
            .Include(cc => cc.IdModalidadCarreraNavigation)
                .ThenInclude(mc => mc.IdCarreraNavigation)
            .Include(cc => cc.IdModalidadCarreraNavigation)
                .ThenInclude(mc => mc.IdModalidadNavigation)
            .Include(cc => cc.TitulModalidadesTitulacionCarreras)
                .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
                    .ThenInclude(m => m.TitulRequisitoModalidad)
            .Where(cc => cc.IdCohorte == cohorte.IdCohorte)
            .ToListAsync(ct);

        var carrerasDto = carreras.Select(cc => new CarreraConvocatoriaDto(
            cc.IdCohorteCarrera,
            cc.IdModalidadCarrera,
            cc.IdModalidadCarreraNavigation?.IdCarrera ?? 0,
            cc.IdModalidadCarreraNavigation?.IdCarreraNavigation?.Carrera ?? "Carrera no identificada",
            cc.IdModalidadCarreraNavigation?.IdModalidad ?? 0,
            cc.IdModalidadCarreraNavigation?.IdModalidadNavigation?.Modalidad ?? "Modalidad no identificada",
            cc.EsActivo ?? false,
            cc.TitulModalidadesTitulacionCarreras.Select(mtc => new ModalidadTitulacionHabilitadaDto(
                mtc.IdModalidadTitulacionCarrera,
                mtc.IdModalidadTitulacion,
                mtc.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? string.Empty,
                mtc.EsActivo ?? false,
                mtc.IdModalidadTitulacionNavigation?.TitulRequisitoModalidad?.Count(r => r.EsActivo) ?? 0
            )).ToList()
        )).ToList();

        return new ConvocatoriaDetalleDto(
            cohorte.IdCohorte,
            cohorte.IdPeriodo ?? string.Empty,
            cohorte.Detelle ?? string.Empty,
            cohorte.FechaInicio,
            cohorte.FechaFin,
            cohorte.DiasPermitidos,
            cohorte.DiasExtension,
            cohorte.EsActivo ?? false,
            vigente,
            carrerasDto
        );
    }
}
