using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.DTOs.Egresados;
using TitulacionIstpet.Application.Interfaces;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EgresadosController(IEgresadosService egresadosService) : ControllerBase
{
    private readonly IEgresadosService _egresadosService = egresadosService;

    /// <summary>
    /// Listar estudiantes que culminaron niveles/materias de su malla pero no están egresados/titulados formalmente.
    /// </summary>
    [HttpGet("pendientes")]
    public async Task<ActionResult<PagedResultDto<EstudiantePendienteEgresoDto>>> GetPendientesEgreso(
        [FromQuery] FiltroPendientesEgresoRequestDto filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await _egresadosService.GetPendientesEgresoAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtener el expediente académico detallado de un estudiante (matrículas, asignaturas, notas y avance).
    /// </summary>
    [HttpGet("expediente/{idAlumno}")]
    public async Task<ActionResult<ExpedienteAcademicoDto>> GetExpedienteAcademico(
        string idAlumno,
        [FromQuery] int? idCarrera,
        CancellationToken cancellationToken)
    {
        var expediente = await _egresadosService.GetExpedienteAcademicoAsync(idAlumno, idCarrera, cancellationToken);
        if (expediente == null)
        {
            return NotFound(new { message = $"No se encontró el estudiante o expediente con identificación {idAlumno}." });
        }

        return Ok(expediente);
    }
}
