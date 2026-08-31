using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Features.Convocatorias.CasosDeUso;
using TitulacionIstpet.Application.Features.Convocatorias.DTOs;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ConvocatoriasController(
    AperturarPeriodoConvocatoria aperturar,
    ConsultarConvocatorias consultar,
    AdministrarConvocatoria administrar) : ControllerBase
{
    private readonly AperturarPeriodoConvocatoria _aperturar = aperturar;
    private readonly ConsultarConvocatorias _consultar = consultar;
    private readonly AdministrarConvocatoria _administrar = administrar;

    /// <summary>
    /// Apertura automatizada de un periodo de titulación con fechas de corte y asignación masiva de carreras
    /// </summary>
    [HttpPost("aperturar")]
    public async Task<ActionResult<int>> AperturarPeriodo(
        [FromBody] AperturarPeriodoConvocatoriaComando comando, CancellationToken ct)
    {
        int idCohorte = await _aperturar.EjecutarAsync(comando, ct);
        var detalle = await _consultar.ObtenerPorIdAsync(idCohorte, ct);
        return CreatedAtAction(nameof(GetActiva), new { id = idCohorte }, detalle);
    }

    /// <summary>
    /// Obtiene la convocatoria/cohorte de titulación activa actualmente en el instituto
    /// </summary>
    [HttpGet("activa")]
    public async Task<ActionResult<ConvocatoriaDetalleDto>> GetActiva(CancellationToken ct)
    {
        var cohorte = await _consultar.ObtenerActivaAsync(ct);
        if (cohorte == null)
        {
            return NotFound(new { message = "No existe una convocatoria de titulación activa en este momento." });
        }
        return Ok(cohorte);
    }

    /// <summary>
    /// Lista el histórico de todas las convocatorias y periodos de titulación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConvocatoriaResumenDto>>> ListarTodas(CancellationToken ct)
    {
        var lista = await _consultar.ListarAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Obtiene el detalle de una convocatoria por su identificador único
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConvocatoriaDetalleDto>> GetPorId(int id, CancellationToken ct)
    {
        var detalle = await _consultar.ObtenerPorIdAsync(id, ct);
        if (detalle == null)
        {
            return NotFound(new { message = $"La convocatoria con ID {id} no fue encontrada." });
        }
        return Ok(detalle);
    }

    /// <summary>
    /// Ajustar o extender fechas de corte de una convocatoria
    /// </summary>
    [HttpPatch("{id:int}/fechas-corte")]
    [HttpPut("{id:int}/fechas-corte")]
    public async Task<IActionResult> AjustarFechasCorte(
        int id, [FromBody] AjustarFechasCorteComando comando, CancellationToken ct)
    {
        if (comando.IdCohorte != id)
        {
            comando = comando with { IdCohorte = id };
        }
        await _administrar.AjustarFechasCorteAsync(comando, ct);
        return NoContent();
    }

    /// <summary>
    /// Habilitar o deshabilitar una modalidad de titulación para una carrera específica en la convocatoria
    /// </summary>
    [HttpPatch("modalidades-carrera/{idModalidadTitulacionCarrera:int}/estado")]
    public async Task<IActionResult> ConmutarModalidadCarrera(
        int idModalidadTitulacionCarrera, [FromQuery] bool activo, CancellationToken ct)
    {
        await _administrar.ConmutarModalidadCarreraAsync(
            new ConmutarModalidadCarreraComando(idModalidadTitulacionCarrera, activo), ct);
        return NoContent();
    }
}
