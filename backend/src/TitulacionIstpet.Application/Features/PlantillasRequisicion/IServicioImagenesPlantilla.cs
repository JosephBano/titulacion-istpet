namespace TitulacionIstpet.Application.Features.PlantillasRequisicion;

public interface IServicioImagenesPlantilla
{
    Task<AdjuntoImagenPlantillaDto> GuardarImagenAsync(
        Stream contenidoStream,
        string nombreArchivoOriginal,
        string contentType,
        long longitudBytes,
        CancellationToken ct = default);

    Task<(Stream ContenidoStream, string ContentType, string NombreArchivo)?> ObtenerImagenAsync(
        int idAdjuntosImagenes,
        CancellationToken ct = default);
}
