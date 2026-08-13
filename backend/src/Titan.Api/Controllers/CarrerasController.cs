using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titan.Application.DTOs.Academico;
using Titan.Application.Interfaces;

namespace Titan.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CarrerasController : ControllerBase
{
    private readonly ICarrerasService _carrerasService;

    public CarrerasController(ICarrerasService carrerasService)
    {
        _carrerasService = carrerasService;
    }

    /// <summary>
    /// Listar todas las carreras activas de la institución
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarreraDto>>> GetCarrerasTodas(CancellationToken cancellationToken)
    {
        var carreras = await _carrerasService.GetCarrerasTodasAsync(cancellationToken);
        return Ok(carreras);
    }

    /// <summary>
    /// Listar las carreras registradas de un estudiante (incluyendo estado de titulación y matrícula por carrera)
    /// </summary>
    [HttpGet("estudiante/{idAlumno}")]
    public async Task<ActionResult<IEnumerable<EstudianteCarreraDto>>> GetCarrerasPorEstudiante(string idAlumno, CancellationToken cancellationToken)
    {
        var carreras = await _carrerasService.GetCarrerasPorEstudianteAsync(idAlumno, cancellationToken);
        return Ok(carreras);
    }

    /// <summary>
    /// Listar las carreras asignadas a un docente (con soporte de asignación a todas o específicas)
    /// </summary>
    [HttpGet("docente/{idProfesor}")]
    public async Task<ActionResult<IEnumerable<ProfesorCarreraDto>>> GetCarrerasPorProfesor(string idProfesor, CancellationToken cancellationToken)
    {
        var carreras = await _carrerasService.GetCarrerasPorProfesorAsync(idProfesor, cancellationToken);
        return Ok(carreras);
    }

    /// <summary>
    /// Obtener el consolidado multicarrera del usuario autenticado (estudiante, docente o ambos)
    /// </summary>
    [HttpGet("mis-carreras")]
    public async Task<ActionResult<UsuarioCarrerasResponseDto>> GetMisCarreras(CancellationToken cancellationToken)
    {
        var idSigafi = User.FindFirstValue("idSigafi") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(idSigafi))
        {
            return BadRequest(new { message = "No se pudo identificar la cédula/código del usuario desde el token." });
        }

        var resultado = await _carrerasService.GetCarrerasUsuarioAutenticadoAsync(idSigafi, cancellationToken);
        if (resultado == null)
        {
            return NotFound(new { message = "No se encontraron registros académicos o laborales de carreras para el usuario." });
        }

        return Ok(resultado);
    }
}
