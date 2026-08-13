using Titan.Application.DTOs.Auth;

namespace Titan.Application.Interfaces;

public interface IRbacService
{
    Task<UserPermissionsDto> BuildUserPermissionsAsync(int idUsuario, string systemCode = "TITAN", CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(int idUsuario, string moduleName, string operationName, CancellationToken cancellationToken = default);
}
