using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.ResponsablesRequisitos;
using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Persistence.Repositories;

public sealed class RepositorioResponsablesRequisitos(SigafiDbContext context) : IRepositorioResponsablesRequisitos
{
    private readonly SigafiDbContext _context = context;

    public async Task<IReadOnlyList<ResponsableRequisitoDto>> ListarPorRequisitoAsync(int idRequisito, CancellationToken ct = default)
    {
        return await _context.TitulResponsableRequisitos
            .AsNoTracking()
            .Include(r => r.IdProfesorNavigation)
            .Include(r => r.IdRequisitosNavigation)
            .Where(r => r.IdRequisitos == idRequisito)
            .Select(r => new ResponsableRequisitoDto(
                r.IdResponsableEvidencias,
                r.IdRequisitos,
                r.IdRequisitosNavigation != null ? (r.IdRequisitosNavigation.Requisito ?? string.Empty) : string.Empty,
                r.IdProfesor,
                r.IdProfesorNavigation != null ? $"{r.IdProfesorNavigation.Nombres} {r.IdProfesorNavigation.Apellidos}".Trim() : r.IdProfesor,
                r.IdProfesorNavigation != null ? (r.IdProfesorNavigation.Email ?? r.IdProfesorNavigation.EmailInstitucional ?? string.Empty) : string.Empty,
                r.IdProfesorNavigation != null ? (r.IdProfesorNavigation.Activo == true) : true
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProfesorCandidatoDto>> ListarProfesoresCandidatosAsync(string? busqueda, CancellationToken ct = default)
    {
        var query = _context.Profesores
            .AsNoTracking()
            .Where(p => p.Activo == true)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim();
            query = query.Where(p =>
                (p.IdProfesor != null && p.IdProfesor.Contains(term)) ||
                (p.Nombres != null && p.Nombres.Contains(term)) ||
                (p.Apellidos != null && p.Apellidos.Contains(term)) ||
                (p.Email != null && p.Email.Contains(term)) ||
                (p.EmailInstitucional != null && p.EmailInstitucional.Contains(term)));
        }

        return await query
            .OrderBy(p => p.Apellidos)
            .ThenBy(p => p.Nombres)
            .Select(p => new ProfesorCandidatoDto(
                p.IdProfesor,
                $"{p.Nombres} {p.Apellidos}".Trim(),
                p.Email ?? p.EmailInstitucional ?? string.Empty,
                p.Celular ?? p.Telefono ?? string.Empty,
                p.Activo == true
            ))
            .ToListAsync(ct);
    }

    public async Task<int> AsignarProfesorAsync(int idRequisitos, string idProfesor, CancellationToken ct = default)
    {
        var existeRequisito = await _context.TitulRequisitos
            .AnyAsync(r => r.IdRequisitos == idRequisitos, ct);

        if (!existeRequisito)
        {
            throw new NoEncontradoException("Requisito maestro", idRequisitos);
        }

        var existeProfesor = await _context.Profesores
            .AnyAsync(p => p.IdProfesor == idProfesor, ct);

        if (!existeProfesor)
        {
            throw new NoEncontradoException("Profesor", idProfesor);
        }

        var yaAsignado = await _context.TitulResponsableRequisitos
            .FirstOrDefaultAsync(r => r.IdRequisitos == idRequisitos && r.IdProfesor == idProfesor, ct);

        if (yaAsignado != null)
        {
            return yaAsignado.IdResponsableEvidencias;
        }

        var nuevaAsignacion = new TitulResponsableRequisitos
        {
            IdRequisitos = idRequisitos,
            IdProfesor = idProfesor
        };

        _context.TitulResponsableRequisitos.Add(nuevaAsignacion);
        await _context.SaveChangesAsync(ct);

        return nuevaAsignacion.IdResponsableEvidencias;
    }

    public async Task DesasignarProfesorAsync(int idResponsableEvidencias, CancellationToken ct = default)
    {
        var asignacion = await _context.TitulResponsableRequisitos
            .FirstOrDefaultAsync(r => r.IdResponsableEvidencias == idResponsableEvidencias, ct)
            ?? throw new NoEncontradoException("Asignación de profesor a requisito", idResponsableEvidencias);

        _context.TitulResponsableRequisitos.Remove(asignacion);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<RequisitoEvaluacionDocenteDto>> ListarPendientesDocenteAsync(string idProfesor, CancellationToken ct = default)
    {
        // 1. Obtener los IDs de requisitos donde este profesor está asignado
        var asignaciones = await _context.TitulResponsableRequisitos
            .AsNoTracking()
            .Where(r => r.IdProfesor == idProfesor)
            .ToListAsync(ct);

        if (asignaciones.Count == 0)
        {
            return Array.Empty<RequisitoEvaluacionDocenteDto>();
        }

        var idsRequisitos = asignaciones.Select(a => a.IdRequisitos).Distinct().ToList();
        var mapAsignacion = asignaciones.ToDictionary(a => a.IdRequisitos, a => a.IdResponsableEvidencias);

        // 2. Buscar requisitos de postulaciones activas para esos requisitos
        var items = await _context.TitulPostulacionAlumnosRequisitosModalidad
            .AsNoTracking()
            .Include(parm => parm.IdPostulacionAlumnosNavigation)
                .ThenInclude(p => p.IdMatriculaNavigation)
                    .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(parm => parm.IdPostulacionAlumnosNavigation)
                .ThenInclude(p => p.IdMatriculaNavigation)
                    .ThenInclude(m => m.IdNivelNavigation)
                        .ThenInclude(n => n.IdCarreraNavigation)
            .Include(parm => parm.IdPostulacionAlumnosNavigation)
                .ThenInclude(p => p.IdModalidadTitulacionCarreraNavigation)
                    .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(parm => parm.IdRequisitoModalidadNavigation)
                .ThenInclude(rm => rm.IdRequisitosNavigation)
            .Include(parm => parm.IdAdjuntosImagenesNavigation)
            .Include(parm => parm.TitulResponsableEvidencia)
            .Where(parm => parm.IdPostulacionAlumnosNavigation.EsActivo == true &&
                           idsRequisitos.Contains(parm.IdRequisitoModalidadNavigation.IdRequisitos))
            .OrderByDescending(parm => parm.IdPostulacionAlumnos)
            .ToListAsync(ct);

        var resultado = new List<RequisitoEvaluacionDocenteDto>();

        foreach (var item in items)
        {
            var p = item.IdPostulacionAlumnosNavigation;
            var alumno = p.IdMatriculaNavigation?.IdAlumnoNavigation;
            var carrera = p.IdMatriculaNavigation?.IdNivelNavigation?.IdCarreraNavigation?.Carrera ?? "Carrera no especificada";
            var modalidad = p.IdModalidadTitulacionCarreraNavigation?.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? "Modalidad no especificada";
            var idReq = item.IdRequisitoModalidadNavigation?.IdRequisitos ?? 0;
            if (idReq == 0) continue;
            var idResp = mapAsignacion.TryGetValue(idReq, out var rId) ? rId : 0;

            var evidenciaDocente = item.TitulResponsableEvidencia
                .OrderByDescending(e => e.IdTitulResponsableEvidencia)
                .FirstOrDefault(e => e.IdResponsableEvidencias == idResp);

            string nombreAlumno = $"{alumno?.PrimerNombre} {alumno?.SegundoNombre} {alumno?.ApellidoPaterno} {alumno?.ApellidoMaterno}".Replace("  ", " ").Trim();

            resultado.Add(new RequisitoEvaluacionDocenteDto(
                IdPostulacionAlumnos: p.IdPostulacionAlumnos,
                IdPostulacionAlumnoRequisitoModalidad: item.IdPostulacionAlumnoRequisitoModalidad,
                IdResponsableEvidencias: idResp,
                IdRequisitos: idReq,
                NombreRequisito: item.IdRequisitoModalidadNavigation?.IdRequisitosNavigation?.Requisito ?? string.Empty,
                IdAlumno: alumno?.IdAlumno ?? string.Empty,
                NombreAlumno: string.IsNullOrWhiteSpace(nombreAlumno) ? (alumno?.IdAlumno ?? "Alumno") : nombreAlumno,
                CedulaAlumno: alumno?.IdAlumno ?? string.Empty,
                Carrera: carrera,
                Modalidad: modalidad,
                EstadoEvaluacion: evidenciaDocente?.Estado ?? (item.ValorBool == true ? "APROBADO" : "PENDIENTE"),
                Observaciones: evidenciaDocente?.Observaciones,
                IdAdjuntosImagenes: item.IdAdjuntosImagenes,
                NombreArchivoAdjunto: item.IdAdjuntosImagenesNavigation?.NombreArchivos,
                RutaArchivoAdjunto: item.IdAdjuntosImagenesNavigation?.Ruta,
                Aprobado: evidenciaDocente?.Estado == "APROBADO" || item.ValorBool == true
            ));
        }

        return resultado;
    }

    public async Task EvaluarRequisitoAsync(EvaluarRequisitoDocenteDto comando, string idEvaluador, CancellationToken ct = default)
    {
        var itemRequisito = await _context.TitulPostulacionAlumnosRequisitosModalidad
            .Include(r => r.TitulResponsableEvidencia)
            .FirstOrDefaultAsync(r => r.IdPostulacionAlumnoRequisitoModalidad == comando.IdPostulacionAlumnoRequisitoModalidad, ct)
            ?? throw new NoEncontradoException("Requisito de postulación", comando.IdPostulacionAlumnoRequisitoModalidad);

        var asignacion = await _context.TitulResponsableRequisitos
            .FirstOrDefaultAsync(a => a.IdResponsableEvidencias == comando.IdResponsableEvidencias, ct)
            ?? throw new NoEncontradoException("Responsable de requisito", comando.IdResponsableEvidencias);

        var now = DateTime.UtcNow;
        var estado = comando.Aprobado ? "APROBADO" : "OBSERVADO";

        var evidencia = itemRequisito.TitulResponsableEvidencia
            .FirstOrDefault(e => e.IdResponsableEvidencias == comando.IdResponsableEvidencias);

        if (evidencia == null)
        {
            evidencia = new TitulResponsableEvidencia
            {
                IdPostulacionAlumnoRequisitoModalidad = itemRequisito.IdPostulacionAlumnoRequisitoModalidad,
                IdResponsableEvidencias = comando.IdResponsableEvidencias,
                Estado = estado,
                Observaciones = comando.Observaciones?.Trim() ?? string.Empty,
                Creado = now,
                Actualizado = now,
                IdCreado = idEvaluador.Length > 14 ? idEvaluador[..14] : idEvaluador,
                IdActualizado = idEvaluador.Length > 14 ? idEvaluador[..14] : idEvaluador
            };
            _context.TitulResponsableEvidencia.Add(evidencia);
        }
        else
        {
            evidencia.Estado = estado;
            evidencia.Observaciones = comando.Observaciones?.Trim() ?? string.Empty;
            evidencia.Actualizado = now;
            evidencia.IdActualizado = idEvaluador.Length > 14 ? idEvaluador[..14] : idEvaluador;
        }

        // Si el docente adjunta un archivo o certificado opcional, actualizar en el requisito
        if (comando.IdAdjuntosImagenes.HasValue && comando.IdAdjuntosImagenes.Value > 0)
        {
            itemRequisito.IdAdjuntosImagenes = comando.IdAdjuntosImagenes.Value;
        }

        // Actualizar ValorBool en el requisito según si está aprobado
        itemRequisito.ValorBool = comando.Aprobado;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<EvaluacionDocenteItemDto>> ListarEvaluacionesPorRequisitoPostulacionAsync(
        int idPostulacionAlumnoRequisitoModalidad, CancellationToken ct = default)
    {
        return await _context.TitulResponsableEvidencia
            .AsNoTracking()
            .Include(e => e.IdResponsableEvidenciasNavigation)
                .ThenInclude(r => r.IdProfesorNavigation)
            .Where(e => e.IdPostulacionAlumnoRequisitoModalidad == idPostulacionAlumnoRequisitoModalidad)
            .OrderByDescending(e => e.Actualizado ?? e.Creado)
            .Select(e => new EvaluacionDocenteItemDto(
                e.IdTitulResponsableEvidencia,
                e.IdPostulacionAlumnoRequisitoModalidad,
                e.IdResponsableEvidencias,
                e.Estado ?? string.Empty,
                e.Observaciones,
                e.Actualizado ?? e.Creado,
                e.IdResponsableEvidenciasNavigation != null && e.IdResponsableEvidenciasNavigation.IdProfesorNavigation != null
                    ? $"{e.IdResponsableEvidenciasNavigation.IdProfesorNavigation.Nombres} {e.IdResponsableEvidenciasNavigation.IdProfesorNavigation.Apellidos}".Trim()
                    : (e.IdActualizado ?? e.IdCreado ?? "Docente Evaluador")
            ))
            .ToListAsync(ct);
    }
}
