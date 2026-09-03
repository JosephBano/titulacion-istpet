using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.Consultas;

public sealed class ListarProfesoresCandidatos(IRepositorioResponsablesRequisitos repositorio)
{
    private readonly IRepositorioResponsablesRequisitos _repositorio = repositorio;

    public Task<IReadOnlyList<ProfesorCandidatoDto>> EjecutarAsync(string? busqueda, CancellationToken ct = default)
    {
        return _repositorio.ListarProfesoresCandidatosAsync(busqueda, ct);
    }
}
