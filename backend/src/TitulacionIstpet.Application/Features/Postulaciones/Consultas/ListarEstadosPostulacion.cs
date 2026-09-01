using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed class ListarEstadosPostulacion(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public Task<IReadOnlyList<EstadoPostulacionDto>> EjecutarAsync(CancellationToken ct = default)
    {
        return _repositorio.ListarEstadosAsync(ct);
    }
}
