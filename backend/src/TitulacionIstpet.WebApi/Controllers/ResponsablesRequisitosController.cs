using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Features.ResponsablesRequisitos.Comandos;
using TitulacionIstpet.Application.Features.ResponsablesRequisitos.Consultas;
using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/responsables-requisitos")]
[Authorize]
public class ResponsablesRequisitosController(
    ListarResponsablesPorRequisito listarResponsables,
    ListarProfesoresCandidatos listarProfesores,
    AsignarProfesorRequisito asignarProfesor,
    DesasignarProfesorRequisito desasignarProfesor,
    ListarPendientesDocente listarPendientes,
    EvaluarRequisitoDocente evaluarRequisito,
    ListarEvaluacionesRequisitoPostulacion listarEvaluaciones) : ControllerBase
{
    private readonly ListarResponsablesPorRequisito _listarResponsables = listarResponsables;
    private readonly ListarProfesoresCandidatos _listarProfesores = listarProfesores;
    private readonly AsignarProfesorRequisito _asignarProfesor = asignarProfesor;
    private readonly DesasignarProfesorRequisito _desasignarProfesor = desasignarProfesor;
    private readonly ListarPendientesDocente _listarPendientes = listarPendientes;
    private readonly EvaluarRequisitoDocente _evaluarRequisito = evaluarRequisito;
    private readonly ListarEvaluacionesRequisitoPostulacion _listarEvaluaciones = listarEvaluaciones;

    /// <summary>
    /// Lista los docentes asignados a un requisito de titulación
    /// </summary>
    [HttpGet("requisito/{idRequisito:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ResponsableRequisitoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResponsableRequisitoDto>>> ListarPorRequisito(
        int idRequisito, CancellationToken ct)
    {
        var resultado = await _listarResponsables.EjecutarAsync(idRequisito, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Lista profesores candidatos activos para asignación a requisitos
    /// </summary>
    [HttpGet("profesores-candidatos")]
    [ProducesResponseType(typeof(IReadOnlyList<ProfesorCandidatoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProfesorCandidatoDto>>> ListarProfesoresCandidatos(
        [FromQuery] string? busqueda, CancellationToken ct)
    {
        var resultado = await _listarProfesores.EjecutarAsync(busqueda, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Asigna un docente a un requisito de titulación
    /// </summary>
    [HttpPost("asignar")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> Asignar(
        [FromBody] AsignarProfesorRequisitoDto dto, CancellationToken ct)
    {
        var comando = new AsignarProfesorRequisitoComando(dto.IdRequisitos, dto.IdProfesor);
        var id = await _asignarProfesor.EjecutarAsync(comando, ct);
        return Ok(new { idResponsableEvidencias = id, message = "Docente asignado exitosamente al requisito." });
    }

    /// <summary>
    /// Remueve la asignación de un docente de un requisito
    /// </summary>
    [HttpDelete("{idResponsableEvidencias:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desasignar(int idResponsableEvidencias, CancellationToken ct)
    {
        await _desasignarProfesor.EjecutarAsync(idResponsableEvidencias, ct);
        return NoContent();
    }

    /// <summary>
    /// Lista los requisitos de postulaciones que el docente autenticado tiene pendientes de evaluar
    /// </summary>
    [HttpGet("docente/mis-pendientes")]
    [ProducesResponseType(typeof(IReadOnlyList<RequisitoEvaluacionDocenteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RequisitoEvaluacionDocenteDto>>> MisPendientes(CancellationToken ct)
    {
        var idProfesor = User.FindFirstValue("idSigafi") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(idProfesor))
        {
            return BadRequest(new { message = "No se pudo identificar al docente desde el token." });
        }

        var resultado = await _listarPendientes.EjecutarAsync(new ListarPendientesDocenteConsulta(idProfesor), ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Registra la evaluación (aprobación/observación y adjunto opcional) de un requisito por parte de un docente
    /// </summary>
    [HttpPost("evaluar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Evaluar(
        [FromBody] EvaluarRequisitoDocenteDto dto, CancellationToken ct)
    {
        var idEvaluador = User.FindFirstValue("idSigafi") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "EVALUADOR";

        var comando = new EvaluarRequisitoDocenteComando(dto, idEvaluador);
        await _evaluarRequisito.EjecutarAsync(comando, ct);
        return NoContent();
    }

    /// <summary>
    /// Obtiene las evaluaciones/observaciones registradas sobre un requisito de postulación específico
    /// </summary>
    [HttpGet("requisito-postulacion/{idPostulacionAlumnoRequisitoModalidad:int}/evaluaciones")]
    [ProducesResponseType(typeof(IReadOnlyList<EvaluacionDocenteItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EvaluacionDocenteItemDto>>> ListarEvaluaciones(
        int idPostulacionAlumnoRequisitoModalidad, CancellationToken ct)
    {
        var resultado = await _listarEvaluaciones.EjecutarAsync(idPostulacionAlumnoRequisitoModalidad, ct);
        return Ok(resultado);
    }
}
