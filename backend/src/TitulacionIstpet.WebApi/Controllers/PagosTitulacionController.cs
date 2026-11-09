using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.DTOs.Pagos;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Constants;
using TitulacionIstpet.Domain.Exceptions;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/pagos-titulacion")]
[Authorize]
public sealed class PagosTitulacionController : ControllerBase
{
    private readonly IPagosTitulacionService _pagosService;
    private readonly IUsuarioActual _usuarioActual;

    public PagosTitulacionController(IPagosTitulacionService pagosService, IUsuarioActual usuarioActual)
    {
        _pagosService = pagosService;
        _usuarioActual = usuarioActual;
    }

    /// <summary>
    /// Listar las parametrizaciones de pago por modalidad (Complexivo, Propuesta Tecnológica, etc.)
    /// </summary>
    [HttpGet("configuraciones")]
    [Authorize(Roles = $"{PagosConstantes.Roles.TitulacionAdmin},{PagosConstantes.Roles.Secretaria},{PagosConstantes.Roles.Contabilidad}")]
    public async Task<ActionResult<IReadOnlyList<ConfiguracionPagoModalidadDto>>> ListarConfiguraciones(
        [FromQuery] string? idPeriodo,
        CancellationToken ct)
    {
        var configs = await _pagosService.ListarConfiguracionesAsync(idPeriodo, ct);
        return Ok(configs);
    }

    /// <summary>
    /// Obtener la parametrización de pago aplicable a una modalidad específica
    /// </summary>
    [HttpGet("configuraciones/modalidad/{idModalidad:int}")]
    [Authorize(Roles = $"{PagosConstantes.Roles.TitulacionAdmin},{PagosConstantes.Roles.Secretaria},{PagosConstantes.Roles.Contabilidad}")]
    public async Task<ActionResult<ConfiguracionPagoModalidadDto>> ObtenerConfiguracionPorModalidad(
        int idModalidad,
        [FromQuery] string? idPeriodo,
        CancellationToken ct)
    {
        var config = await _pagosService.ObtenerConfiguracionPorModalidadAsync(idModalidad, idPeriodo, ct);
        if (config == null)
        {
            return NotFound(new { message = $"No se encontró modalidad con ID {idModalidad}." });
        }
        return Ok(config);
    }

    /// <summary>
    /// Actualizar o crear la parametrización de topes para una modalidad (Solo Titulación Admin)
    /// </summary>
    [HttpPost("configuraciones")]
    [Authorize(Roles = PagosConstantes.Roles.TitulacionAdmin)]
    public async Task<ActionResult<ConfiguracionPagoModalidadDto>> GuardarConfiguracion(
        [FromBody] GuardarConfiguracionPagoModalidadDto dto,
        CancellationToken ct)
    {
        int usuarioId = ObtenerUsuarioId();
        var guardada = await _pagosService.GuardarConfiguracionAsync(dto, usuarioId, ct);
        return Ok(guardada);
    }

    /// <summary>
    /// Obtener el resumen financiero y estado de pagos de una postulación
    /// </summary>
    [HttpGet("postulacion/{idPostulacionAlumno:int}/resumen")]
    [Authorize(Roles = $"{PagosConstantes.Roles.TitulacionAdmin},{PagosConstantes.Roles.Secretaria},{PagosConstantes.Roles.Contabilidad},TITULACION_ESTUDIANTE,alumno,estudiante")]
    public async Task<ActionResult<ResumenFinancieroEstudianteDto>> ObtenerResumenFinanciero(
        int idPostulacionAlumno,
        CancellationToken ct)
    {
        var resumen = await _pagosService.ObtenerResumenFinancieroEstudianteAsync(idPostulacionAlumno, ct);
        return Ok(resumen);
    }

    /// <summary>
    /// Registrar un nuevo comprobante de pago de titulación (Solo Secretaría o Titulación Admin)
    /// </summary>
    [HttpPost("registrar")]
    [Authorize(Roles = $"{PagosConstantes.Roles.TitulacionAdmin},{PagosConstantes.Roles.Secretaria}")]
    public async Task<ActionResult<PagoTitulacionDto>> RegistrarPago(
        [FromBody] RegistrarPagoTitulacionDto dto,
        CancellationToken ct)
    {
        int usuarioId = ObtenerUsuarioId();
        var pago = await _pagosService.RegistrarPagoAsync(dto, usuarioId, ct);
        return CreatedAtAction(nameof(ObtenerResumenFinanciero), new { idPostulacionAlumno = dto.IdPostulacionAlumno }, pago);
    }

    /// <summary>
    /// Bandeja de comprobantes por revisar para el área de Contabilidad
    /// </summary>
    [HttpGet("contabilidad/bandeja")]
    [Authorize(Roles = $"{PagosConstantes.Roles.Contabilidad},{PagosConstantes.Roles.TitulacionAdmin}")]
    public async Task<ActionResult<IReadOnlyList<PagoTitulacionDto>>> ListarBandejaContabilidad(
        [FromQuery] string? estado,
        CancellationToken ct)
    {
        var pagos = await _pagosService.ListarBandejaContabilidadAsync(estado, ct);
        return Ok(pagos);
    }

    /// <summary>
    /// Aprobar o rechazar un pago de titulación (Acción exclusiva de Contabilidad)
    /// </summary>
    [HttpPut("contabilidad/{idPagoTitulacion:int}/revision")]
    [Authorize(Roles = PagosConstantes.Roles.Contabilidad)]
    public async Task<ActionResult<PagoTitulacionDto>> ProcesarRevisionContabilidad(
        int idPagoTitulacion,
        [FromBody] RevisionPagoDto revision,
        CancellationToken ct)
    {
        int usuarioId = ObtenerUsuarioId();
        var pagoActualizado = await _pagosService.ProcesarRevisionContabilidadAsync(idPagoTitulacion, revision, usuarioId, ct);
        return Ok(pagoActualizado);
    }

    /// <summary>
    /// Consultar el historial de auditoría de cambios de estado de un comprobante
    /// </summary>
    [HttpGet("{idPagoTitulacion:int}/historial")]
    [Authorize(Roles = $"{PagosConstantes.Roles.TitulacionAdmin},{PagosConstantes.Roles.Contabilidad},{PagosConstantes.Roles.Secretaria}")]
    public async Task<ActionResult<IReadOnlyList<HistorialCambioEstadoPagoDto>>> ObtenerHistorial(
        int idPagoTitulacion,
        CancellationToken ct)
    {
        var historial = await _pagosService.ObtenerHistorialPagoAsync(idPagoTitulacion, ct);
        return Ok(historial);
    }

    private int ObtenerUsuarioId()
    {
        if (int.TryParse(_usuarioActual.UserId, out var id) && id > 0)
        {
            return id;
        }

        var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("id")?.Value;

        if (int.TryParse(claimId, out var idClaim) && idClaim > 0)
        {
            return idClaim;
        }

        return 1;
    }
}
