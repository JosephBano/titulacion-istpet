using Titan.Application.DTOs.Auth;

namespace Titan.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string reason, CancellationToken cancellationToken = default);
    Task<UserPermissionsDto> GetUserPermissionsAsync(int idUsuario, string systemCode = "TITAN", CancellationToken cancellationToken = default);
}
