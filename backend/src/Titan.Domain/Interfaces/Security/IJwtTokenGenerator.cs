using Titan.Domain.Entities;

namespace Titan.Domain.Interfaces.Security;

public interface IJwtTokenGenerator
{
    (string accessToken, DateTime expiresAt) GenerateAccessToken(usuarios usuario, IEnumerable<string> roles, IEnumerable<string> permisos);
    string GenerateRefreshToken();
    string HashToken(string token);
}
