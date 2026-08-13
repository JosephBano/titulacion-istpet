using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;

/// <summary>
/// Baja de un adjunto. Comprueba existencia antes de marcar el borrado
/// para devolver 404 explicito cuando el id no existe (sin esto, EF Core
/// reporta 0 filas afectadas y el cliente no sabria si fallo el borrado
/// o si el id era incorrecto).
/// </summary>
public sealed class EliminarAdjunto(
    IRepositorioAdjuntosImagenes repositorio, IUnitOfWork unitOfWork)
{
    private readonly IRepositorioAdjuntosImagenes _repositorio = repositorio;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task EjecutarAsync(int id, CancellationToken ct = default)
    {
        var entidad = await _repositorio.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException(nameof(AdjuntosImagene), id);

        _repositorio.Eliminar(entidad);
        await _unitOfWork.GuardarCambiosAsync(ct);
    }
}
