using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.Consultas;

public sealed class ListarResponsablesPorRequisito(IRepositorioResponsablesRequisitos repositorio)
{
    private readonly IRepositorioResponsablesRequisitos _repositorio = repositorio;

    public Task<IReadOnlyList<ResponsableRequisitoDto>> EjecutarAsync(int idRequisito, CancellationToken ct = default)
    {
        return _repositorio.ListarPorRequisitoAsync(idRequisito, ct);
    }
}
