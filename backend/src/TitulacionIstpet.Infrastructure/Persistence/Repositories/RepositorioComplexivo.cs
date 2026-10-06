using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Complexivo;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public sealed class RepositorioComplexivo(SigafiDbContext context) : IRepositorioComplexivo
{
    public async Task<IReadOnlyList<CohorteCarreraComplexivoDto>> ListarCohortesCarrerasAsync(CancellationToken ct)
        => await context.TitulCohortesCarreras.AsNoTracking()
            .OrderByDescending(c => c.IdCohorte).ThenBy(c => c.IdModalidadCarreraNavigation.IdCarreraNavigation.Carrera)
            .Select(c => new CohorteCarreraComplexivoDto(
                c.IdCohorteCarrera, c.IdCohorte, c.IdModalidadCarreraNavigation.IdCarrera,
                c.IdCohorteNavigation.Detalle ?? string.Empty, c.IdCohorteNavigation.IdPeriodo,
                c.IdModalidadCarreraNavigation.IdCarreraNavigation.Carrera ?? string.Empty,
                c.IdModalidadCarreraNavigation.IdModalidadNavigation.Modalidad ?? string.Empty,
                c.EsActivo == true && c.IdCohorteNavigation.EsActivo == true))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MallaComplexivoDto>> ListarMallaAsync(int idCohorteCarrera, CancellationToken ct)
    {
        if (!await context.TitulCohortesCarreras.AnyAsync(c => c.IdCohorteCarrera == idCohorteCarrera, ct))
        {
            throw new NoEncontradoException("cohorte/carrera", idCohorteCarrera);
        }
        var mallas = await ConsultaMalla().AsNoTracking().Where(m => m.IdCohorteCarrera == idCohorteCarrera)
            .OrderBy(m => m.FechaInicio).ThenBy(m => m.IdMallaComplexivo).ToListAsync(ct);
        return mallas.Select(Mapear).ToList();
    }

    public async Task<MallaComplexivoDto?> ObtenerAsync(int id, CancellationToken ct)
    {
        var malla = await ConsultaMalla().AsNoTracking().FirstOrDefaultAsync(m => m.IdMallaComplexivo == id, ct);
        return malla == null ? null : Mapear(malla);
    }

    public async Task<int> GuardarAsync(int? id, GuardarMallaComplexivoDto dto, CancellationToken ct)
    {
        var cohorteCarrera = await context.TitulCohortesCarreras.AsNoTracking()
            .Where(c => c.IdCohorteCarrera == dto.IdCohorteCarrera)
            .Select(c => new { c.IdModalidadCarreraNavigation.IdCarrera }).FirstOrDefaultAsync(ct)
            ?? throw new NoEncontradoException("cohorte/carrera", dto.IdCohorteCarrera);

        var malla = id.HasValue
            ? await ConsultaMalla().FirstOrDefaultAsync(m => m.IdMallaComplexivo == id, ct)
                ?? throw new NoEncontradoException("malla complexivo", id.Value)
            : new TitulMallaComplexivo { IdCohorteCarrera = dto.IdCohorteCarrera, Modulo = new TitulModuloComplexivo() };

        if (malla.IdCohorteCarrera != dto.IdCohorteCarrera)
        {
            Fallo("IdCohorteCarrera", "La configuración no pertenece a la cohorte y carrera seleccionadas.");
        }
        var idsAsignaturas = dto.Temas.Select(t => t.IdAsignatura).Distinct().ToArray();
        var asignaturasValidas = await context.Detallemallas.AsNoTracking()
            .Where(d => d.IdMallaNavigation.IdCarrera == cohorteCarrera.IdCarrera && idsAsignaturas.Contains(d.IdAsignatura))
            .Select(d => d.IdAsignatura).Distinct().ToListAsync(ct);
        if (asignaturasValidas.Count != idsAsignaturas.Length)
        {
            Fallo("Temas", "Todas las asignaturas deben pertenecer a la malla académica de la carrera.");
        }
        var profesoresNuevos = dto.Docentes
            .Where(d => !string.IsNullOrWhiteSpace(d.IdProfesor) &&
                !malla.Docentes.Any(actual => actual.IdMallaDocenteComplexivo == d.IdMallaDocenteComplexivo && actual.IdProfesor == d.IdProfesor))
            .Select(d => d.IdProfesor!).Distinct().ToArray();
        var profesoresValidos = await context.Profesores.AsNoTracking()
            .CountAsync(p => profesoresNuevos.Contains(p.IdProfesor) && p.Activo == true, ct);
        if (profesoresValidos != profesoresNuevos.Length)
        {
            Fallo("Docentes", "Los nuevos docentes asignados deben existir y estar activos.");
        }

        ValidarIds(dto.Temas.Select(t => t.IdTemas), malla.Modulo.Temas.Select(t => t.IdTemas), "Temas");
        ValidarIds(dto.Docentes.Select(d => d.IdMallaDocenteComplexivo), malla.Docentes.Select(d => d.IdMallaDocenteComplexivo), "Docentes");
        foreach (var docente in dto.Docentes)
        {
            var actual = malla.Docentes.FirstOrDefault(d => d.IdMallaDocenteComplexivo == docente.IdMallaDocenteComplexivo);
            ValidarIds(docente.Horarios.Select(h => h.IdModulosHorarios),
                actual?.Horarios.Select(h => h.IdModulosHorarios) ?? [], "Horarios");
        }

        // Una edición afecta únicamente esta cohorte, incluso si un módulo fue compartido por otro sistema.
        bool clonarModulo = id.HasValue && await context.MallasComplexivo
            .AnyAsync(m => m.IdModulosComplexivo == malla.IdModulosComplexivo && m.IdMallaComplexivo != id, ct);
        if (clonarModulo)
        {
            malla.Modulo = new TitulModuloComplexivo();
        }
        var modulo = malla.Modulo;
        modulo.IdCarrera = cohorteCarrera.IdCarrera;
        modulo.Detalle = dto.DetalleModulo.Trim();
        modulo.FechaInicio = dto.FechaInicio;
        modulo.FechaFin = dto.FechaFin;
        modulo.Modalidad = dto.Modalidad.Trim();
        modulo.TotalHoras = dto.TotalHoras;
        modulo.EsActivo = dto.EsActivo;
        modulo.FechaCreacion ??= DateTime.UtcNow;
        modulo.FechaDesactivacion = dto.EsActivo ? null : modulo.FechaDesactivacion ?? DateTime.UtcNow;

        foreach (var tema in modulo.Temas.Where(t => !dto.Temas.Any(d => d.IdTemas == t.IdTemas)).ToList())
        {
            context.Remove(tema);
            modulo.Temas.Remove(tema);
        }
        foreach (var entrada in dto.Temas)
        {
            var tema = clonarModulo || entrada.IdTemas == 0
                ? new TitulTemaComplexivo()
                : modulo.Temas.Single(t => t.IdTemas == entrada.IdTemas);
            tema.IdAsignatura = entrada.IdAsignatura;
            tema.Detalle = entrada.Detalle.Trim();
            tema.HorasPorTema = entrada.HorasPorTema;
            if (tema.IdTemas == 0)
            {
                modulo.Temas.Add(tema);
            }
        }

        malla.Detalle = dto.Detalle.Trim();
        malla.FechaInicio = dto.FechaInicio;
        malla.FechaFin = dto.FechaFin;
        malla.EsActivo = dto.EsActivo;
        foreach (var docente in malla.Docentes.Where(d => !dto.Docentes.Any(e => e.IdMallaDocenteComplexivo == d.IdMallaDocenteComplexivo)).ToList())
        {
            context.RemoveRange(docente.Horarios);
            context.Remove(docente);
            malla.Docentes.Remove(docente);
        }
        foreach (var entrada in dto.Docentes)
        {
            var docente = entrada.IdMallaDocenteComplexivo == 0
                ? new TitulMallaDocenteComplexivo()
                : malla.Docentes.Single(d => d.IdMallaDocenteComplexivo == entrada.IdMallaDocenteComplexivo);
            docente.IdProfesor = string.IsNullOrWhiteSpace(entrada.IdProfesor) ? null : entrada.IdProfesor;
            docente.FechaAsignacion = entrada.FechaAsignacion;
            docente.CodigoAsignacion = entrada.CodigoAsignacion?.Trim();
            docente.EsActivo = entrada.EsActivo;
            foreach (var horario in docente.Horarios.Where(h => !entrada.Horarios.Any(e => e.IdModulosHorarios == h.IdModulosHorarios)).ToList())
            {
                context.Remove(horario);
                docente.Horarios.Remove(horario);
            }
            foreach (var h in entrada.Horarios)
            {
                var horario = h.IdModulosHorarios == 0 ? new TitulModuloHorarioComplexivo()
                    : docente.Horarios.Single(actual => actual.IdModulosHorarios == h.IdModulosHorarios);
                horario.Dias = h.Dias.Trim();
                horario.FranjaHoraria = h.FranjaHoraria.Trim();
                if (horario.IdModulosHorarios == 0)
                {
                    docente.Horarios.Add(horario);
                }
            }
            if (docente.IdMallaDocenteComplexivo == 0)
            {
                malla.Docentes.Add(docente);
            }
        }
        if (!id.HasValue)
        {
            context.MallasComplexivo.Add(malla);
        }
        // EF Core guarda todo el agregado en una transacción, sin escrituras parciales.
        await context.SaveChangesAsync(ct);
        return malla.IdMallaComplexivo;
    }

    private IQueryable<TitulMallaComplexivo> ConsultaMalla() => context.MallasComplexivo
        .Include(m => m.Modulo).ThenInclude(m => m.Temas)
        .Include(m => m.Docentes).ThenInclude(d => d.Horarios).AsSplitQuery();

    private static MallaComplexivoDto Mapear(TitulMallaComplexivo m) => new(
        m.IdMallaComplexivo, m.IdModulosComplexivo, m.IdCohorteCarrera, m.Detalle,
        m.Modulo.Detalle, m.FechaInicio, m.FechaFin, m.Modulo.Modalidad ?? string.Empty,
        m.Modulo.TotalHoras ?? 0, m.EsActivo,
        m.Modulo.Temas.OrderBy(t => t.IdTemas).Select(t => new TemaComplexivoDto(
            t.IdTemas, t.IdAsignatura, t.Detalle ?? string.Empty, t.HorasPorTema ?? 0)).ToList(),
        m.Docentes.OrderBy(d => d.IdMallaDocenteComplexivo).Select(d => new DocenteComplexivoDto(
            d.IdMallaDocenteComplexivo, d.IdProfesor, d.FechaAsignacion, d.CodigoAsignacion, d.EsActivo == true,
            d.Horarios.OrderBy(h => h.IdModulosHorarios).Select(h => new HorarioComplexivoDto(
                h.IdModulosHorarios, h.Dias ?? string.Empty, h.FranjaHoraria ?? string.Empty)).ToList())).ToList());

    private static void ValidarIds(IEnumerable<int> recibidos, IEnumerable<int> existentes, string campo)
    {
        if (recibidos.Any(id => id > 0 && !existentes.Contains(id)))
        {
            Fallo(campo, "Un registro enviado no pertenece a esta configuración.");
        }
    }

    private static void Fallo(string campo, string mensaje)
        => throw new ValidacionException([new ValidationFailure(campo, mensaje)]);
}
