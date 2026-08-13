using FluentValidation.Results;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;

public sealed record CrearAdjuntoComando(
    string? NombreArchivos,
    string? Extension,
    string? MimeTypes,
    int? TamanioBytes,
    string? Ruta);

/// <summary>
/// Alta de un adjunto. El orden de operaciones es deliberado:
///   1. validar la entrada (lanza <see cref="ValidacionException"/> -> 400),
///   2. marcar la entidad en el ChangeTracker (sin tocar la base),
///   3. confirmar via <see cref="IUnitOfWork"/> (un solo SaveChanges).
///
/// Asi un fallo de validacion nunca deja un INSERT a medias, y dos operaciones
/// de la misma peticion comparten transaccion.
/// </summary>
public sealed class CrearAdjunto
{
    private const int MaxNombre = 90;
    private const int MaxMime = 90;
    private const int MaxExtension = 90;
    private const int MaxRuta = 255;

    private readonly IRepositorioAdjuntosImagenes _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public CrearAdjunto(
        IRepositorioAdjuntosImagenes repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> EjecutarAsync(CrearAdjuntoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var fallos = Validar(comando);
        if (fallos.Count > 0)
        {
            throw new ValidacionException(fallos);
        }

        var entidad = new AdjuntosImagene
        {
            NombreArchivos = comando.NombreArchivos?.Trim(),
            Extension = comando.Extension?.Trim(),
            MimeTypes = comando.MimeTypes?.Trim(),
            TamanioBytes = comando.TamanioBytes,
            Ruta = comando.Ruta?.Trim()
        };

        _repositorio.Agregar(entidad);
        await _unitOfWork.GuardarCambiosAsync(ct);

        return entidad.IdAdjuntosImagenes;
    }

    private static List<ValidationFailure> Validar(CrearAdjuntoComando c)
    {
        var f = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(c.NombreArchivos))
        {
            f.Add(new ValidationFailure(nameof(c.NombreArchivos), "El nombre del archivo es obligatorio."));
        }
        else if (c.NombreArchivos.Length > MaxNombre)
        {
            f.Add(new ValidationFailure(nameof(c.NombreArchivos), $"El nombre no puede superar {MaxNombre} caracteres."));
        }

        if (c.Extension is { Length: > MaxExtension })
        {
            f.Add(new ValidationFailure(nameof(c.Extension), $"La extension no puede superar {MaxExtension} caracteres."));
        }

        if (c.MimeTypes is { Length: > MaxMime })
        {
            f.Add(new ValidationFailure(nameof(c.MimeTypes), $"El MIME no puede superar {MaxMime} caracteres."));
        }

        if (c.Ruta is { Length: > MaxRuta })
        {
            f.Add(new ValidationFailure(nameof(c.Ruta), $"La ruta no puede superar {MaxRuta} caracteres."));
        }

        if (c.TamanioBytes is < 0)
        {
            f.Add(new ValidationFailure(nameof(c.TamanioBytes), "El tamano en bytes no puede ser negativo."));
        }

        return f;
    }
}
