using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.DTOs.Academico;
using TitulacionIstpet.Application.Interfaces;

namespace TitulacionIstpet.WebApi.Controllers;

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
    /// Listar todas las modalidades base (Presencial, Semipresencial, En Línea)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModalidadDto>>> GetModalidades(CancellationToken cancellationToken)
    {
        var modalidades = await _modalidadesService.GetModalidadesAsync(cancellationToken);
        return Ok(modalidades);
    }

    /// <summary>
    /// Listar todos los sistemas/mecanismos de titulación institucionales activos
    /// </summary>
    [HttpGet("sistemas-titulacion")]
    public async Task<ActionResult<IEnumerable<SistemaTitulacionDto>>> GetSistemasTitulacion(CancellationToken cancellationToken)
    {
        var sistemas = await _modalidadesService.GetSistemasTitulacionAsync(cancellationToken);
        return Ok(sistemas);
    }

    /// <summary>
    /// Listar las modalidades habilitadas para todas las carreras
    /// </summary>
    [HttpGet("carreras")]
    public async Task<ActionResult<IEnumerable<ModalidadCarreraDto>>> GetModalidadesCarrerasTodas(CancellationToken cancellationToken)
    {
        var modalidades = await _modalidadesService.GetModalidadesCarrerasTodasAsync(cancellationToken);
        return Ok(modalidades);
    }

    /// <summary>
    /// Listar las modalidades habilitadas específicamente para una carrera
    /// </summary>
    [HttpGet("carreras/carrera/{idCarrera:int}")]
    public async Task<ActionResult<IEnumerable<ModalidadCarreraDto>>> GetModalidadesPorCarrera(int idCarrera, CancellationToken cancellationToken)
    {
        var modalidades = await _modalidadesService.GetModalidadesPorCarreraAsync(idCarrera, cancellationToken);
        return Ok(modalidades);
    }

    /// <summary>
    /// Obtener el contexto de modalidad y titulación del estudiante autenticado
    /// </summary>
    [HttpGet("mi-contexto")]
    public async Task<ActionResult<EstudianteModalidadContextDto>> GetMiContexto(CancellationToken cancellationToken)
    {
        var idAlumno = User.FindFirst("idSigafi")?.Value ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(idAlumno))
        {
            return BadRequest(new { message = "No se pudo identificar al alumno desde el token de autenticación." });
        }

        var contexto = await _modalidadesService.GetContextoModalidadesEstudianteAsync(idAlumno, cancellationToken);
        if (contexto == null)
        {
            return NotFound(new { message = "No se encontró registro de carrera o matrícula activa para el estudiante." });
        }

        return Ok(contexto);
    }
}
