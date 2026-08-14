using TitulacionIstpet.Application.DTOs.Auth;

namespace TitulacionIstpet.Application.Interfaces;

public interface IRbacManagementService
{
    Task<List<RbacSistemaDto>> GetSistemasAsync(CancellationToken cancellationToken = default);
    Task<List<RbacModuloDto>> GetModulosBySistemaAsync(int idSistema, CancellationToken cancellationToken = default);
    Task<List<RbacRolDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RbacRolDto> CreateRolAsync(string nombre, string codigoRol, CancellationToken cancellationToken = default);
    Task<bool> AssignRolToUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default);
    Task<bool> RemoveRolFromUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default);
    Task<bool> AssignPermissionToRolAsync(int idRol, int idModuloOperacion, CancellationToken cancellationToken = default);
}
