using Titan.Domain.Entities;

namespace Titan.Application.Interfaces;

public interface IRbacManagementService
{
    Task<List<rbac_sistema>> GetSistemasAsync(CancellationToken cancellationToken = default);
    Task<List<rbac_modulos>> GetModulosBySistemaAsync(int idSistema, CancellationToken cancellationToken = default);
    Task<List<rbac_rol>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<rbac_rol> CreateRolAsync(string nombre, string codigoRol, CancellationToken cancellationToken = default);
    Task<bool> AssignRolToUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default);
    Task<bool> RemoveRolFromUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default);
    Task<bool> AssignPermissionToRolAsync(int idRol, int idModuloOperacion, CancellationToken cancellationToken = default);
}
