using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Features.AdjuntosImagenes;

/// <summary>
/// Contrato de persistencia para la tabla <c>adjuntos_imagenes</c>.
///
/// Add/Update/Remove son sincronos: solo marcan la entidad como Adjuntar/
/// Modificar/Eliminar en el ChangeTracker del DbContext. La confirmacion
/// ocurre en <see cref="Common.Interfaces.IUnitOfWork.GuardarCambiosAsync"/>,
/// una sola vez por caso de uso, para que las cuatro operaciones compartan
/// la misma transaccion.
/// </summary>
public interface IRepositorioAdjuntosImagenes
{
    Task<AdjuntosImagene?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<AdjuntosImagene>> ListarAsync(
        int pagina, int tamanoPagina, CancellationToken ct = default);

    Task<int> ContarAsync(CancellationToken ct = default);

    void Agregar(AdjuntosImagene entidad);

    void Actualizar(AdjuntosImagene entidad);

    void Eliminar(AdjuntosImagene entidad);
}
