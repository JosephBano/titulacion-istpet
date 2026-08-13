using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titan.Application.DTOs.Academico;
using Titan.Application.Interfaces;

namespace Titan.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ModalidadesController : ControllerBase
{
    private readonly IModalidadesService _modalidadesService;

    public ModalidadesController(IModalidadesService modalidadesService)
    {
        _modalidadesService = modalidadesService;
    }

    /// <summary>
    /// Listar todas las modalidades de estudio (Presencial, Semipresencial, En Línea, etc.)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModalidadDto>>> GetModalidades(CancellationToken cancellationToken)
    {
        var modalidades = await _modalidadesService.GetModalidadesAsync(cancellationToken);
        return Ok(modalidades);
    }

    /// <summary>
    /// Listar todos los sistemas/mecanismos de titulación habilitados (Examen Complexivo, Proyecto de Investigación, etc.)
    /// </summary>
    [HttpGet("sistemas-titulacion")]
    public async Task<ActionResult<IEnumerable<SistemaTitulacionDto>>> GetSistemasTitulacion(CancellationToken cancellationToken)
    {
        var sistemas = await _modalidadesService.GetSistemasTitulacionAsync(cancellationToken);
        return Ok(sistemas);
    }

    /// <summary>
    /// Obtener detalle de una modalidad de estudio por su ID
    /// </summary>
    [HttpGet("{idModalidad:int}")]
    public async Task<ActionResult<ModalidadDto>> GetModalidadPorId(int idModalidad, CancellationToken cancellationToken)
    {
        var modalidad = await _modalidadesService.GetModalidadPorIdAsync(idModalidad, cancellationToken);
        if (modalidad == null)
        {
            return NotFound(new { message = $"La modalidad con ID {idModalidad} no fue encontrada." });
        }

        return Ok(modalidad);
    }

    /// <summary>
    /// Listar la asignación global de modalidades por carrera
    /// </summary>
    [HttpGet("carreras")]
    public async Task<ActionResult<IEnumerable<ModalidadCarreraDto>>> GetModalidadesCarrerasTodas(CancellationToken cancellationToken)
    {
        var modalidadesCarreras = await _modalidadesService.GetModalidadesCarrerasTodasAsync(cancellationToken);
        return Ok(modalidadesCarreras);
    }

    /// <summary>
    /// Listar modalidades asignadas a una carrera específica
    /// </summary>
    [HttpGet("carreras/carrera/{idCarrera:int}")]
    public async Task<ActionResult<IEnumerable<ModalidadCarreraDto>>> GetModalidadesPorCarrera(int idCarrera, CancellationToken cancellationToken)
    {
        var modalidades = await _modalidadesService.GetModalidadesPorCarreraAsync(idCarrera, cancellationToken);
        return Ok(modalidades);
    }

    /// <summary>
    /// Obtener el contexto completo de modalidades y sistemas de titulación para el estudiante autenticado
    /// </summary>
    [HttpGet("mi-contexto")]
    public async Task<ActionResult<EstudianteModalidadContextDto>> GetMiContextoModalidades(CancellationToken cancellationToken)
    {
        var idAlumno = User.FindFirstValue("idSigafi") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(idAlumno))
        {
            return BadRequest(new { message = "No se pudo identificar la cédula/código del estudiante desde el token." });
        }

        var contexto = await _modalidadesService.GetContextoModalidadesEstudianteAsync(idAlumno, cancellationToken);
        if (contexto == null)
        {
            return NotFound(new { message = "No se encontraron registros académicos de carrera o matrícula para el estudiante." });
        }

        return Ok(contexto);
    }
}
