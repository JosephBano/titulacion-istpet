using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;

namespace TitulacionIstpet.WebApi.Controllers;

/// <summary>
/// CRUD de la tabla <c>adjuntos_imagenes</c>. Sirve de ejemplo para futuros
/// controllers: capa fina, sin logica, que solo traduce HTTP a un caso de uso
/// de Application. Toda la validacion y las excepciones de negocio viven en
/// el caso de uso, y <c>ManejadorExcepcionesMiddleware</c> las traduce a 400
/// (ValidacionException), 404 (NoEncontradoException), 409 (DominioException)
/// o 500.
///
/// El controller no inyecta <c>SigafiDbContext</c>, ni el repositorio: si los
/// pidiera, la regla de arquitectura <c>Controllers_NoDebenDependerDe_EfCore</c>
/// romperia el build en CI.
/// </summary>
[ApiController]
[Route("api/adjuntos-imagenes")]
public sealed class AdjuntosImagenesController : ControllerBase
{
    private readonly ObtenerAdjuntoPorId _obtener;
    private readonly ListarAdjuntos _listar;
    private readonly CrearAdjunto _crear;
    private readonly ActualizarAdjunto _actualizar;
    private readonly EliminarAdjunto _eliminar;

    public AdjuntosImagenesController(
        ObtenerAdjuntoPorId obtener,
        ListarAdjuntos listar,
        CrearAdjunto crear,
        ActualizarAdjunto actualizar,
        EliminarAdjunto eliminar)
    {
        _obtener = obtener;
        _listar = listar;
        _crear = crear;
        _actualizar = actualizar;
        _eliminar = eliminar;
    }

    [HttpGet]
    public Task<PaginaAdjuntos> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = ListarAdjuntos.TamanoPaginaPorDefecto,
        CancellationToken ct = default)
        => _listar.EjecutarAsync(new ListarAdjuntosConsulta(pagina, tamanoPagina), ct);

    [HttpGet("{id:int}")]
    public Task<AdjuntosImageneDto> Obtener(int id, CancellationToken ct)
        => _obtener.EjecutarAsync(new ObtenerAdjuntoPorIdConsulta(id), ct);

    [HttpPost]
    public async Task<ActionResult<AdjuntosImageneDto>> Crear(
        [FromBody] CrearAdjuntoComando comando, CancellationToken ct)
    {
        var id = await _crear.EjecutarAsync(comando, ct);

        // 201 Created con Location al recurso recien creado. El cliente puede
        // seguir el header para pedir el detalle sin tener que construir la URL.
        var dto = await _obtener.EjecutarAsync(new ObtenerAdjuntoPorIdConsulta(id), ct);
        return CreatedAtAction(nameof(Obtener), new { id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarAdjuntoComando comando, CancellationToken ct)
    {
        // El id de la URL manda. Asi no hay forma de que un cliente actualice
        // un registro distinto al que apunta.
        if (comando.IdAdjuntosImagenes != id)
        {
            comando = comando with { IdAdjuntosImagenes = id };
        }

        await _actualizar.EjecutarAsync(comando, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _eliminar.EjecutarAsync(id, ct);
        return NoContent();
    }
}
