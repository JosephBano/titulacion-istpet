using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Features.PlantillasRequisicion;
using TitulacionIstpet.WebApi.Attributes;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/complexivo/plantilla-requisicion")]
[Authorize(Roles = "TITULACION_ADMIN")]
public sealed class PlantillaRequisicionController(
    AdministrarPlantillaRequisicion administrar,
    IServicioImagenesPlantilla servicioImagenes,
    IUsuarioActual usuarioActual) : ControllerBase
{
    [HttpGet]
    [HasPermission("Examen Complexivo", "ver")]
    public async Task<ActionResult<PlantillaRequisicionEnvelopeDto>> Obtener(CancellationToken ct)
    {
        var resultado = await administrar.ObtenerPlantillaAsync(ct);
        return Ok(resultado);
    }

    [HttpPut]
    [HasPermission("Examen Complexivo", "editar")]
    public async Task<ActionResult<PlantillaRequisicionEnvelopeDto>> Actualizar(
        [FromBody] ActualizarPlantillaRequisicionComandoDto comando,
        CancellationToken ct)
    {
        int? usuarioId = int.TryParse(usuarioActual.UserId, out var parsedId) ? parsedId : null;
        var resultado = await administrar.ActualizarPlantillaAsync(comando, usuarioId, ct);
        return Ok(resultado);
    }

    [HttpPost("imagenes")]
    [Consumes("multipart/form-data")]
    [HasPermission("Examen Complexivo", "editar")]
    public async Task<ActionResult<AdjuntoImagenPlantillaDto>> SubirImagen(
        IFormFile? archivo,
        CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { message = "No se ha proporcionado un archivo válido." });
        }

        await using var stream = archivo.OpenReadStream();
        var resultado = await servicioImagenes.GuardarImagenAsync(
            stream,
            archivo.FileName,
            archivo.ContentType,
            archivo.Length,
            ct);

        return Ok(resultado);
    }

    [HttpGet("imagenes/{id:int}")]
    [HasPermission("Examen Complexivo", "ver")]
    public async Task<IActionResult> ObtenerImagen(int id, CancellationToken ct)
    {
        var recurso = await servicioImagenes.ObtenerImagenAsync(id, ct);
        if (recurso == null)
        {
            return NotFound();
        }

        return File(recurso.Value.ContenidoStream, recurso.Value.ContentType, recurso.Value.NombreArchivo);
    }
}
