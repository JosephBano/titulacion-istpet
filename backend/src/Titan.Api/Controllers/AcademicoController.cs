using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titan.Application.DTOs.Academico;
using Titan.Application.Interfaces;

namespace Titan.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AcademicoController : ControllerBase
{
    private readonly IAcademicoService _academicoService;

    public AcademicoController(IAcademicoService academicoService)
    {
        _academicoService = academicoService;
    }

    /// <summary>
    /// Listar las carreras activas en la institución
    /// </summary>
    [HttpGet("carreras")]
    public async Task<ActionResult<IEnumerable<CarreraResponseDto>>> GetCarreras(CancellationToken cancellationToken)
    {
        var carreras = await _academicoService.GetCarrerasActivasAsync(cancellationToken);
        return Ok(carreras);
    }

    /// <summary>
    /// Obtener detalle de una carrera por ID
    /// </summary>
    [HttpGet("carreras/{idCarrera:int}")]
    public async Task<ActionResult<CarreraResponseDto>> GetCarreraPorId(int idCarrera, CancellationToken cancellationToken)
    {
        var carrera = await _academicoService.GetCarreraPorIdAsync(idCarrera, cancellationToken);
        if (carrera == null)
        {
            return NotFound(new { message = $"La carrera con ID {idCarrera} no existe." });
        }

        return Ok(carrera);
    }

    /// <summary>
    /// Listar períodos lectivos vigentes
    /// </summary>
    [HttpGet("periodos")]
    public async Task<ActionResult<IEnumerable<PeriodoResponseDto>>> GetPeriodos(CancellationToken cancellationToken)
    {
        var periodos = await _academicoService.GetPeriodosVigentesAsync(cancellationToken);
        return Ok(periodos);
    }

    /// <summary>
    /// Listar asignaturas asociadas a las mallas de una carrera
    /// </summary>
    [HttpGet("carreras/{idCarrera:int}/asignaturas")]
    public async Task<ActionResult<IEnumerable<AsignaturaResponseDto>>> GetAsignaturas(int idCarrera, CancellationToken cancellationToken)
    {
        var asignaturas = await _academicoService.GetAsignaturasPorMallaAsync(idCarrera, cancellationToken);
        return Ok(asignaturas);
    }

    /// <summary>
    /// Listar las modalidades de estudio (Presencial, Semipresencial, En Línea)
    /// </summary>
    [HttpGet("modalidades")]
    public async Task<ActionResult<IEnumerable<ModalidadResponseDto>>> GetModalidades(CancellationToken cancellationToken)
    {
        var modalidades = await _academicoService.GetModalidadesAsync(cancellationToken);
        return Ok(modalidades);
    }
}
