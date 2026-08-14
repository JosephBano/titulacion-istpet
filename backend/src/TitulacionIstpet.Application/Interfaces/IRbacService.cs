using TitulacionIstpet.Application.DTOs.Auth;

namespace TitulacionIstpet.Application.Interfaces;

public interface IRbacService
{
    Task<UserPermissionsDto> BuildUserPermissionsAsync(int idUsuario, string systemCode = "TITULACION", CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(int idUsuario, string moduleName, string operationName, CancellationToken cancellationToken = default);
}
