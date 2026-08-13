namespace Titan.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
