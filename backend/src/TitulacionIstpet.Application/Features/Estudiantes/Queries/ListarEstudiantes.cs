using MediatR;
using TitulacionIstpet.Domain.Repositories;

namespace TitulacionIstpet.Application.Features.Estudiantes.Queries;

public record ListarEstudiantesQuery : IRequest<IReadOnlyList<EstudianteDto>>;

public class ListarEstudiantesHandler
    : IRequestHandler<ListarEstudiantesQuery, IReadOnlyList<EstudianteDto>>
{
    private readonly IEstudianteRepository _repo;

    public ListarEstudiantesHandler(IEstudianteRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<EstudianteDto>> Handle(
        ListarEstudiantesQuery request, CancellationToken cancellationToken)
    {
        var estudiantes = await _repo.ListarAsync(cancellationToken);
        return estudiantes.Select(EstudianteDto.Desde).ToList();
    }
}
