using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Features.Postulaciones.Comandos;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PostulacionesController(
    ConsultarElegibilidadEstudiante consultarElegibilidad,
    ListarModalidadesOfertadas listarModalidadesOfertadas,
    ObtenerMiPostulacion obtenerMiPostulacion,
    ObtenerPostulacionPorId obtenerPorId,
    ListarPostulaciones listarPostulaciones,
    ListarEstadosPostulacion listarEstados,
    CrearPostulacion crearPostulacion,
    ActualizarRequisitosPostulacion actualizarRequisitos,
    CambiarEstadoPostulacion cambiarEstado,
    SolicitarCambioModalidad solicitarCambioModalidad,
    ObtenerPortalEstudiante obtenerPortalEstudiante,
    DictaminarPostulacion dictaminarPostulacion) : ControllerBase
{
    private readonly ConsultarElegibilidadEstudiante _consultarElegibilidad = consultarElegibilidad;
    private readonly ListarModalidadesOfertadas _listarModalidadesOfertadas = listarModalidadesOfertadas;
    private readonly ObtenerMiPostulacion _obtenerMiPostulacion = obtenerMiPostulacion;
    private readonly ObtenerPostulacionPorId _obtenerPorId = obtenerPorId;
    private readonly ListarPostulaciones _listarPostulaciones = listarPostulaciones;
    private readonly ListarEstadosPostulacion _listarEstados = listarEstados;
    private readonly CrearPostulacion _crearPostulacion = crearPostulacion;
    private readonly ActualizarRequisitosPostulacion _actualizarRequisitos = actualizarRequisitos;
    private readonly CambiarEstadoPostulacion _cambiarEstado = cambiarEstado;
    private readonly SolicitarCambioModalidad _solicitarCambioModalidad = solicitarCambioModalidad;
    private readonly ObtenerPortalEstudiante _obtenerPortalEstudiante = obtenerPortalEstudiante;
    private readonly DictaminarPostulacion _dictaminarPostulacion = dictaminarPostulacion;

    /// <summary>
    /// Consulta el estado de elegibilidad y modalidades disponibles del estudiante autenticado
    /// </summary>
    [HttpGet("elegibilidad")]
    [ProducesResponseType(typeof(ElegibilidadPostulacionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ElegibilidadPostulacionDto>> GetElegibilidad(CancellationToken ct)
    {
        var idAlumno = User.FindFirstValue("idSigafi") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(idAlumno))
        {
            return BadRequest(new { message = "No se pudo identificar al alumno desde el token de autenticación." });
        }

        var resultado = await _consultarElegibilidad.EjecutarAsync(new ConsultarElegibilidadEstudianteConsulta(idAlumno), ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene la postulación activa del estudiante autenticado con sus requisitos y adjuntos
    /// </summary>
    [HttpGet("mi-postulacion")]
    [ProducesResponseType(typeof(PostulacionDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostulacionDetalleDto>> GetMiPostulacion(CancellationToken ct)
    {
        var idAlumno = User.FindFirstValue("idSigafi") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(idAlumno))
        {
            return BadRequest(new { message = "No se pudo identificar al alumno desde el token de autenticación." });
        }

        var postulacion = await _obtenerMiPostulacion.EjecutarAsync(new ObtenerMiPostulacionConsulta(idAlumno), ct);
        if (postulacion == null)
        {
            return NotFound(new { message = "No registra una postulación activa en el periodo vigente." });
        }

        return Ok(postulacion);
    }

    /// <summary>
    /// Lista las modalidades y requisitos ofertados para una cohorte y carrera
    /// </summary>
    [HttpGet("modalidades-ofertadas/{idCohorteCarrera:int}")]
    [ProducesResponseType(typeof(IEnumerable<ModalidadOfertadaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ModalidadOfertadaDto>>> GetModalidadesOfertadas(
        int idCohorteCarrera, CancellationToken ct)
    {
        var modalidades = await _listarModalidadesOfertadas.EjecutarAsync(
            new ListarModalidadesOfertadasConsulta(idCohorteCarrera), ct);
        return Ok(modalidades);
    }

    /// <summary>
    /// Catálogo ordenado de estados de postulación
    /// </summary>
    [HttpGet("estados")]
    [ProducesResponseType(typeof(IEnumerable<EstadoPostulacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EstadoPostulacionDto>>> GetEstados(CancellationToken ct)
    {
        var estados = await _listarEstados.EjecutarAsync(ct);
        return Ok(estados);
    }

    /// <summary>
    /// Listado general paginado de postulaciones con filtros dinámicos (Coordinador / Comisión / Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaPostulacionesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaPostulacionesDto>> Listar(
        [FromQuery] int? idCohorte,
        [FromQuery] int? idCarrera,
        [FromQuery] int? idModalidad,
        [FromQuery] int? idEstado,
        [FromQuery] string? busqueda,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken ct = default)
    {
        var consulta = new ListarPostulacionesConsulta(
            IdCohorte: idCohorte,
            IdCarrera: idCarrera,
            IdModalidad: idModalidad,
            IdEstado: idEstado,
            Busqueda: busqueda,
            Pagina: pagina,
            TamanoPagina: tamanoPagina
        );

        var resultado = await _listarPostulaciones.EjecutarAsync(consulta, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene el detalle completo de una postulación por identificador único
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PostulacionDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostulacionDetalleDto>> GetPorId(int id, CancellationToken ct)
    {
        var resultado = await _obtenerPorId.EjecutarAsync(new ObtenerPostulacionPorIdConsulta(id), ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Registrar una nueva postulación a titulación
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PostulacionDetalleDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PostulacionDetalleDto>> Crear(
        [FromBody] CrearPostulacionComando comando, CancellationToken ct)
    {
        int idCreado = await _crearPostulacion.EjecutarAsync(comando, ct);
        var dto = await _obtenerPorId.EjecutarAsync(new ObtenerPostulacionPorIdConsulta(idCreado), ct);
        return CreatedAtAction(nameof(GetPorId), new { id = idCreado }, dto);
    }

    /// <summary>
    /// Actualizar o subir documentos y respuestas de requisitos de una postulación
    /// </summary>
    [HttpPut("{id:int}/requisitos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActualizarRequisitos(
        int id, [FromBody] ActualizarRequisitosPostulacionComando comando, CancellationToken ct)
    {
        if (comando.IdPostulacionAlumnos != id)
        {
            comando = comando with { IdPostulacionAlumnos = id };
        }

        await _actualizarRequisitos.EjecutarAsync(comando, ct);
        return NoContent();
    }

    /// <summary>
    /// Cambiar el estado de una postulación (Aprobar / Observar / Rechazar)
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarEstado(
        int id, [FromBody] CambiarEstadoPostulacionComando comando, CancellationToken ct)
    {
        if (comando.IdPostulacionAlumnos != id)
        {
            comando = comando with { IdPostulacionAlumnos = id };
        }

        await _cambiarEstado.EjecutarAsync(comando, ct);
        return NoContent();
    }

    /// <summary>
    /// Solicitar cambio de modalidad de titulación en una postulación activa
    /// </summary>
    [HttpPost("{id:int}/solicitar-cambio-modalidad")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SolicitarCambioModalidad(
        int id, [FromBody] SolicitarCambioModalidadComando comando, CancellationToken ct)
    {
        if (comando.IdPostulacionAlumnos != id)
        {
            comando = comando with { IdPostulacionAlumnos = id };
        }

        await _solicitarCambioModalidad.EjecutarAsync(comando, ct);
        return NoContent();
    }

    /// <summary>
    /// Consulta consolidada del portal del estudiante (Convocatoria, Elegibilidad, Modalidades y Postulación)
    /// </summary>
    [HttpGet("mi-portal")]
    [ProducesResponseType(typeof(PortalEstudianteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortalEstudianteDto>> GetMiPortal(CancellationToken ct)
    {
        var idAlumno = User.FindFirstValue("idSigafi") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(idAlumno))
        {
            return BadRequest(new { message = "No se pudo identificar al alumno desde el token de autenticación." });
        }

        var portal = await _obtenerPortalEstudiante.EjecutarAsync(new ObtenerPortalEstudianteConsulta(idAlumno), ct);
        return Ok(portal);
    }

    /// <summary>
    /// Dictaminar en un solo paso una postulación (Aprobar / Observar / Rechazar con feedback)
    /// </summary>
    [HttpPost("{id:int}/dictamen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Dictaminar(
        int id, [FromBody] DictamenPostulacionComando comando, CancellationToken ct)
    {
        if (comando.IdPostulacionAlumnos != id)
        {
            comando = comando with { IdPostulacionAlumnos = id };
        }

        await _dictaminarPostulacion.EjecutarAsync(comando, ct);
        return NoContent();
    }
}
