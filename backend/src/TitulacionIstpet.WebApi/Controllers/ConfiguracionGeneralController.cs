using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Features.ConfiguracionGeneral.CasosDeUso;
using TitulacionIstpet.Application.Features.ConfiguracionGeneral.DTOs;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/configuracion")]
[Authorize]
public class ConfiguracionGeneralController(
    ListarConfiguracionGeneral listar,
    AdministrarModalidades adminModalidades,
    AdministrarRequisitos adminRequisitos,
    AdministrarMatrizRequisitosModalidad adminMatriz) : ControllerBase
{
    private readonly ListarConfiguracionGeneral _listar = listar;
    private readonly AdministrarModalidades _adminModalidades = adminModalidades;
    private readonly AdministrarRequisitos _adminRequisitos = adminRequisitos;
    private readonly AdministrarMatrizRequisitosModalidad _adminMatriz = adminMatriz;

    #region Resumen General y Estado del Sistema
    /// <summary>
    /// Obtener resumen general y estado operativo del sistema de titulación en 1 sola llamada
    /// </summary>
    [HttpGet("resumen-general")]
    public async Task<ActionResult<ResumenGeneralSistemaDto>> ObtenerResumenGeneral(CancellationToken ct = default)
    {
        var resumen = await _listar.ObtenerResumenGeneralAsync(ct);
        return Ok(resumen);
    }
    #endregion

    #region Modalidades Maestras
    [HttpGet("modalidades")]
    public async Task<ActionResult<IReadOnlyList<ModalidadMaestraDto>>> ListarModalidades(
        [FromQuery] bool soloActivas = false, CancellationToken ct = default)
    {
        var result = await _listar.ListarModalidadesAsync(soloActivas, ct);
        return Ok(result);
    }

    [HttpPost("modalidades")]
    public async Task<ActionResult<int>> CrearModalidad(
        [FromBody] CrearModalidadMaestraDto dto, CancellationToken ct)
    {
        var id = await _adminModalidades.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(ListarModalidades), new { id }, id);
    }

    [HttpPut("modalidades/{id:int}")]
    public async Task<IActionResult> ActualizarModalidad(
        int id, [FromBody] ActualizarModalidadMaestraDto dto, CancellationToken ct)
    {
        if (dto.IdModalidadTitulacion != id)
        {
            dto = dto with { IdModalidadTitulacion = id };
        }
        await _adminModalidades.ActualizarAsync(dto, ct);
        return NoContent();
    }

    [HttpPatch("modalidades/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoModalidad(
        int id, [FromQuery] bool activo, CancellationToken ct)
    {
        await _adminModalidades.CambiarEstadoAsync(id, activo, ct);
        return NoContent();
    }
    #endregion

    #region Requisitos Maestros
    [HttpGet("requisitos")]
    public async Task<ActionResult<IReadOnlyList<RequisitoMaestroDto>>> ListarRequisitos(
        [FromQuery] bool soloActivos = false, CancellationToken ct = default)
    {
        var result = await _listar.ListarRequisitosAsync(soloActivos, ct);
        return Ok(result);
    }

    [HttpPost("requisitos")]
    public async Task<ActionResult<int>> CrearRequisito(
        [FromBody] CrearRequisitoMaestroDto dto, CancellationToken ct)
    {
        var id = await _adminRequisitos.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(ListarRequisitos), new { id }, id);
    }

    [HttpPut("requisitos/{id:int}")]
    public async Task<IActionResult> ActualizarRequisito(
        int id, [FromBody] ActualizarRequisitoMaestroDto dto, CancellationToken ct)
    {
        if (dto.IdRequisitos != id)
        {
            dto = dto with { IdRequisitos = id };
        }
        await _adminRequisitos.ActualizarAsync(dto, ct);
        return NoContent();
    }

    [HttpPatch("requisitos/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoRequisito(
        int id, [FromQuery] bool activo, CancellationToken ct)
    {
        await _adminRequisitos.CambiarEstadoAsync(id, activo, ct);
        return NoContent();
    }
    #endregion

    #region Matriz Requisitos - Modalidad
    [HttpGet("modalidades/{idModalidad:int}/requisitos")]
    public async Task<ActionResult<IReadOnlyList<RequisitoModalidadMatrizDto>>> ListarRequisitosPorModalidad(
        int idModalidad, CancellationToken ct)
    {
        var result = await _listar.ListarRequisitosPorModalidadAsync(idModalidad, ct);
        return Ok(result);
    }

    [HttpPost("modalidades/{idModalidad:int}/requisitos/{idRequisito:int}")]
    public async Task<ActionResult<int>> AsignarRequisitoAModalidad(
        int idModalidad, int idRequisito, [FromQuery] bool esRequisitoFinal = false, CancellationToken ct = default)
    {
        var dto = new AsignarRequisitoModalidadDto(idModalidad, idRequisito, esRequisitoFinal);
        var id = await _adminMatriz.AsignarAsync(dto, ct);
        return Ok(new { idRequisitoModalidad = id, message = "Requisito asociado exitosamente a la modalidad." });
    }

    [HttpDelete("modalidades/requisitos/{idRequisitoModalidad:int}")]
    public async Task<IActionResult> DesasignarRequisitoDeModalidad(
        int idRequisitoModalidad, CancellationToken ct)
    {
        await _adminMatriz.DesasignarAsync(idRequisitoModalidad, ct);
        return NoContent();
    }
    #endregion
}
