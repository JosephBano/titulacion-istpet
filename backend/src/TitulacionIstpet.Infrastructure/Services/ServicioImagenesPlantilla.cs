using Microsoft.AspNetCore.Hosting;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.PlantillasRequisicion;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Infrastructure.Services;

public sealed class ServicioImagenesPlantilla(
    IWebHostEnvironment env,
    IRepositorioAdjuntosImagenes repoAdjuntos,
    IUnitOfWork unitOfWork) : IServicioImagenesPlantilla
{
    private const long MaximoTamanoBytes = 5 * 1024 * 1024; // 5 MiB
    private static readonly HashSet<string> ExtensionesPermitidas = [".png", ".jpg", ".jpeg"];
    private static readonly HashSet<string> MimeTypesPermitidos = ["image/png", "image/jpeg"];

    public async Task<AdjuntoImagenPlantillaDto> GuardarImagenAsync(
        Stream contenidoStream,
        string nombreArchivoOriginal,
        string contentType,
        long longitudBytes,
        CancellationToken ct = default)
    {
        if (longitudBytes <= 0 || contenidoStream == null)
        {
            throw new ValidacionException([new FluentValidation.Results.ValidationFailure("archivo", "No se ha proporcionado un archivo válido.")]);
        }

        if (longitudBytes > MaximoTamanoBytes)
        {
            throw new ValidacionException([new FluentValidation.Results.ValidationFailure("archivo", "El tamaño de la imagen no debe superar los 5 MiB.")]);
        }

        var ext = Path.GetExtension(nombreArchivoOriginal).ToLowerInvariant();
        var mime = (contentType ?? string.Empty).ToLowerInvariant();

        if (!ExtensionesPermitidas.Contains(ext) || !MimeTypesPermitidos.Contains(mime))
        {
            throw new ValidacionException([new FluentValidation.Results.ValidationFailure("archivo", "Solo se permiten imágenes en formato PNG o JPEG.")]);
        }

        var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;

        var plantillasDir = Path.Combine(webRoot, "plantillas");
        if (!Directory.Exists(plantillasDir))
        {
            Directory.CreateDirectory(plantillasDir);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(plantillasDir, uniqueFileName);

        await using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await contenidoStream.CopyToAsync(fileStream, ct);
        }

        var rutaRelativa = $"/plantillas/{uniqueFileName}";
        var nombreCorto = nombreArchivoOriginal.Length > 85 ? nombreArchivoOriginal[..85] : nombreArchivoOriginal;
        var extLimpia = ext.TrimStart('.');

        var adjunto = new AdjuntosImagenes
        {
            NombreArchivos = nombreCorto,
            Extension = extLimpia.Length > 20 ? extLimpia[..20] : extLimpia,
            MimeTypes = mime.Length > 85 ? mime[..85] : mime,
            TamanioBytes = (int)Math.Min(longitudBytes, int.MaxValue),
            Ruta = rutaRelativa
        };

        repoAdjuntos.Agregar(adjunto);
        await unitOfWork.GuardarCambiosAsync(ct);

        return new AdjuntoImagenPlantillaDto(
            IdAdjuntosImagenes: adjunto.IdAdjuntosImagenes,
            NombreArchivo: adjunto.NombreArchivos,
            MimeType: adjunto.MimeTypes,
            TamanoBytes: longitudBytes,
            Url: $"/api/v1/complexivo/plantilla-requisicion/imagenes/{adjunto.IdAdjuntosImagenes}"
        );
    }

    public async Task<(Stream ContenidoStream, string ContentType, string NombreArchivo)?> ObtenerImagenAsync(
        int idAdjuntosImagenes,
        CancellationToken ct = default)
    {
        var adjunto = await repoAdjuntos.ObtenerPorIdAsync(idAdjuntosImagenes, ct);
        if (adjunto == null)
        {
            return null;
        }

        var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;

        var rutaNormalizada = (adjunto.Ruta ?? string.Empty).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(webRoot, rutaNormalizada);

        if (!File.Exists(physicalPath))
        {
            return null;
        }

        var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var mime = string.IsNullOrWhiteSpace(adjunto.MimeTypes) ? "application/octet-stream" : adjunto.MimeTypes;
        var nombre = string.IsNullOrWhiteSpace(adjunto.NombreArchivos) ? "imagen" : adjunto.NombreArchivos;

        return (stream, mime, nombre);
    }
}
