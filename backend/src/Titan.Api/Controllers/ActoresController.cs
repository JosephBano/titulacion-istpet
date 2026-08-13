using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titan.Application.DTOs.Actores;
using Titan.Application.Interfaces;

namespace Titan.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ActoresController : ControllerBase
{
    private readonly IActoresService _actoresService;

    public ActoresController(IActoresService actoresService)
    {
        _actoresService = actoresService;
    }

    /// <summary>
    /// Buscar alumnos por cédula, nombre o correo institucional
    /// </summary>
    [HttpGet("alumnos")]
    public async Task<ActionResult<IEnumerable<AlumnoResponseDto>>> BuscarAlumnos([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var alumnos = await _actoresService.BuscarAlumnosAsync(q, cancellationToken);
        return Ok(alumnos);
    }

    /// <summary>
    /// Obtener detalle del alumno por su número de cédula/ID
    /// </summary>
    [HttpGet("alumnos/{cedula}")]
    public async Task<ActionResult<AlumnoResponseDto>> GetAlumnoPorCedula(string cedula, CancellationToken cancellationToken)
    {
        var alumno = await _actoresService.GetAlumnoPorCedulaAsync(cedula, cancellationToken);
        if (alumno == null)
        {
            return NotFound(new { message = $"El estudiante con cédula {cedula} no fue encontrado." });
        }

        return Ok(alumno);
    }

    /// <summary>
    /// Listar docentes evaluadores activos (disponibles para tutores o tribunales)
    /// </summary>
    [HttpGet("docentes")]
    public async Task<ActionResult<IEnumerable<ProfesorResponseDto>>> GetDocentes(CancellationToken cancellationToken)
    {
        var docentes = await _actoresService.GetDocentesEvaluadoresAsync(cancellationToken);
        return Ok(docentes);
    }

    /// <summary>
    /// Obtener detalle de docente por cédula
    /// </summary>
    [HttpGet("docentes/{cedula}")]
    public async Task<ActionResult<ProfesorResponseDto>> GetDocentePorCedula(string cedula, CancellationToken cancellationToken)
    {
        var docente = await _actoresService.GetDocentePorCedulaAsync(cedula, cancellationToken);
        if (docente == null)
        {
            return NotFound(new { message = $"El docente con cédula {cedula} no fue encontrado." });
        }

        return Ok(docente);
    }

    /// <summary>
    /// Obtener matrículas registradas de un alumno
    /// </summary>
    [HttpGet("alumnos/{cedula}/matriculas")]
    public async Task<ActionResult<IEnumerable<MatriculaResponseDto>>> GetMatriculasPorAlumno(string cedula, CancellationToken cancellationToken)
    {
        var matriculas = await _actoresService.GetMatriculasPorAlumnoAsync(cedula, cancellationToken);
        return Ok(matriculas);
    }

    /// <summary>
    /// Validar la aptitud de titulación de un estudiante en una carrera determinada
    /// </summary>
    [HttpGet("alumnos/{cedula}/aptitud/{idCarrera:int}")]
    public async Task<ActionResult<AptitudTitulacionResponseDto>> ValidarAptitud(string cedula, int idCarrera, CancellationToken cancellationToken)
    {
        var aptitud = await _actoresService.ValidarAptitudTitulacionAsync(cedula, idCarrera, cancellationToken);
        return Ok(aptitud);
    }

    /// <summary>
    /// Listar alumnos aptos para titulación (filtrados y excluyendo titulados)
    /// </summary>
    [HttpGet("alumnos/aptos-titulacion")]
    public async Task<ActionResult<IEnumerable<AlumnoAptoDto>>> GetAlumnosAptos(
        [FromQuery] int? idCarrera,
        [FromQuery] int? idModalidad,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var alumnos = await _actoresService.GetAlumnosAptosTitulacionAsync(idCarrera, idModalidad, q, cancellationToken);
        return Ok(alumnos);
    }

    /// <summary>
    /// Listar registro de alumnos graduados e históricos (alumnos_titulos)
    /// </summary>
    [HttpGet("alumnos/graduados")]
    public async Task<ActionResult<IEnumerable<GraduadoHistoricoDto>>> GetAlumnosGraduados(
        [FromQuery] int? idCarrera,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var graduados = await _actoresService.GetAlumnosGraduadosAsync(idCarrera, q, cancellationToken);
        return Ok(graduados);
    }
}
