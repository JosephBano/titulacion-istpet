namespace TitulacionIstpet.Application.Features.AdjuntosImagenes;

/// <summary>
/// Forma externa del adjunto. No expone las navegaciones de la entidad (las seis
/// colecciones de FKs inversa que el scaffold incluye) para que un cambio en el
/// modelo relacional no se filtre al contrato HTTP.
/// </summary>
public sealed record AdjuntosImageneDto(
    int IdAdjuntosImagenes,
    string? NombreArchivos,
    string? Extension,
    string? MimeTypes,
    int? TamanioBytes,
    string? Ruta);
