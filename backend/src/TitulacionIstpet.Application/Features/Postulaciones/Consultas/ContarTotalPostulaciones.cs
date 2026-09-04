using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed class ContarTotalPostulaciones(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public async Task<TotalPostulacionesDto> EjecutarAsync(CancellationToken ct = default)
    {
        int total = await _repositorio.ContarTotalPostulacionesAsync(ct);
        return new TotalPostulacionesDto(total);
    }
}
