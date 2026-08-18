using TitulacionIstpet.Application.Common.Models;

namespace TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;

public sealed record ObtenerAdjuntoPorIdConsulta(int Id);

/// <summary>
/// Devuelve un adjunto por su id. Mapea a 404 via <see cref="NoEncontradoException"/>
/// cuando el id no existe; asi el controlador no tiene que distinguir entre
/// "no encontrado" y "exito".
/// </summary>
public sealed class ObtenerAdjuntoPorId(IRepositorioAdjuntosImagenes repositorio)
{
    private readonly IRepositorioAdjuntosImagenes _repositorio = repositorio;

    public async Task<AdjuntosImageneDto> EjecutarAsync(
        ObtenerAdjuntoPorIdConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);

        var entidad = await _repositorio.ObtenerPorIdAsync(consulta.Id, ct)
            ?? throw new NoEncontradoException(nameof(Domain.Entities.AdjuntosImagenes), consulta.Id);

        return AdjuntosImageneMapeo.A_DTO(entidad);
    }
}
