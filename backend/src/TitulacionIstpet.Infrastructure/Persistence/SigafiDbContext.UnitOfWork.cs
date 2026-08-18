using TitulacionIstpet.Application.Common.Interfaces;

namespace TitulacionIstpet.Infrastructure.Persistence;

/// <summary>
/// Clase parcial que extiende el SigafiDbContext auto-generado para implementar
/// IUnitOfWork. Al ser partial, sobrevive a las regeneraciones del scaffold: solo
/// se sobreescribe SigafiDbContext.cs (la parte auto-generada), mientras que este
/// archivo permanece intacto.
/// </summary>
public partial class SigafiDbContext : IUnitOfWork
{
    /// <inheritdoc/>
    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) =>
        SaveChangesAsync(ct);
}
