using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Features.Estudiantes;
using TitulacionIstpet.Application.Features.Estudiantes.Commands;
using TitulacionIstpet.Application.Features.Estudiantes.Queries;

namespace TitulacionIstpet.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstudiantesController : ControllerBase
{
    private readonly ISender _sender;

    public EstudiantesController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EstudianteDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct) =>
        Ok(await _sender.Send(new ListarEstudiantesQuery(), ct));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(CrearEstudianteCommand comando, CancellationToken ct)
    {
        var id = await _sender.Send(comando, ct);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }
}
