namespace TitulacionIstpet.Application.Features.AdjuntosImagenes;

/// <summary>
/// Conversion unica entre <see cref="Domain.Entities.AdjuntosImagene"/> y
/// <see cref="AdjuntosImageneDto"/>. Vive en Application porque el dominio
/// no debe conocer la forma externa, y la propia forma externa no debe
/// arrastrar EF Core para mapear.
/// </summary>
internal static class AdjuntosImageneMapeo
{
    public static AdjuntosImageneDto A_DTO(Domain.Entities.AdjuntosImagene e) => new(
        e.IdAdjuntosImagenes,
        e.NombreArchivos,
        e.Extension,
        e.MimeTypes,
        e.TamanioBytes,
        e.Ruta);
}
