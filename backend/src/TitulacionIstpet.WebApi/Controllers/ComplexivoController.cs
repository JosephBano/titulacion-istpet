using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Features.Complexivo;
using TitulacionIstpet.WebApi.Attributes;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/complexivo")]
[Authorize(Roles = "TITULACION_ADMIN")]
public sealed class ComplexivoController(AdministrarComplexivo administrar) : ControllerBase
{
    [HttpGet("cohortes-carreras")]
    [HasPermission("Examen Complexivo", "ver")]
    public async Task<ActionResult<IReadOnlyList<CohorteCarreraComplexivoDto>>> ListarCohortesCarreras(CancellationToken ct)
        => Ok(await administrar.ListarCohortesCarrerasAsync(ct));

    [HttpGet("malla")]
    [HasPermission("Examen Complexivo", "ver")]
    public async Task<ActionResult<IReadOnlyList<MallaComplexivoDto>>> ListarMalla([FromQuery] int idCohorteCarrera, CancellationToken ct)
        => Ok(await administrar.ListarMallaAsync(idCohorteCarrera, ct));

    [HttpGet("malla/{id:int}")]
    [HasPermission("Examen Complexivo", "ver")]
    public async Task<ActionResult<MallaComplexivoDto>> Obtener(int id, CancellationToken ct)
    {
        var malla = await administrar.ObtenerAsync(id, ct);
        return malla == null ? NotFound() : Ok(malla);
    }

    [HttpPost("malla")]
    [HasPermission("Examen Complexivo", "crear")]
    public async Task<ActionResult<MallaComplexivoDto>> Crear([FromBody] GuardarMallaComplexivoDto dto, CancellationToken ct)
    {
        var id = await administrar.GuardarAsync(null, dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id }, await administrar.ObtenerAsync(id, ct));
    }

    [HttpPut("malla/{id:int}")]
    [HasPermission("Examen Complexivo", "editar")]
    public async Task<ActionResult<MallaComplexivoDto>> Actualizar(int id, [FromBody] GuardarMallaComplexivoDto dto, CancellationToken ct)
    {
        await administrar.GuardarAsync(id, dto, ct);
        return Ok(await administrar.ObtenerAsync(id, ct));
    }
}
