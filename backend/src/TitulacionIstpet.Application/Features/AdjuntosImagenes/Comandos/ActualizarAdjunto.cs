using FluentValidation.Results;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;

public sealed record ActualizarAdjuntoComando(
    int IdAdjuntosImagenes,
    string? NombreArchivos,
    string? Extension,
    string? MimeTypes,
    int? TamanioBytes,
    string? Ruta);

/// <summary>
/// Modificacion parcial de un adjunto. Las validaciones son las mismas que
/// en el alta: el registro debe existir (404 si no) y los campos respetar
/// los limites de la columna. La entidad se carga por el repositorio,
/// muta en memoria, y persiste con un unico <see cref="IUnitOfWork"/>.
/// </summary>
public sealed class ActualizarAdjunto
{
    private const int MaxNombre = 90;
    private const int MaxMime = 90;
    private const int MaxExtension = 90;
    private const int MaxRuta = 255;

    private readonly IRepositorioAdjuntosImagenes _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarAdjunto(
        IRepositorioAdjuntosImagenes repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(ActualizarAdjuntoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var fallos = Validar(comando);
        if (fallos.Count > 0)
        {
            throw new ValidacionException(fallos);
        }

        var entidad = await _repositorio.ObtenerPorIdAsync(comando.IdAdjuntosImagenes, ct)
            ?? throw new NoEncontradoException(nameof(AdjuntosImagene), comando.IdAdjuntosImagenes);

        entidad.NombreArchivos = comando.NombreArchivos?.Trim();
        entidad.Extension = comando.Extension?.Trim();
        entidad.MimeTypes = comando.MimeTypes?.Trim();
        entidad.TamanioBytes = comando.TamanioBytes;
        entidad.Ruta = comando.Ruta?.Trim();

        _repositorio.Actualizar(entidad);
        await _unitOfWork.GuardarCambiosAsync(ct);
    }

    private static List<ValidationFailure> Validar(ActualizarAdjuntoComando c)
    {
        var f = new List<ValidationFailure>();

        if (c.IdAdjuntosImagenes <= 0)
        {
            f.Add(new ValidationFailure(nameof(c.IdAdjuntosImagenes), "El id es obligatorio."));
        }

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
