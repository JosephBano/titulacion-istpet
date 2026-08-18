using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Domain.Interfaces.Security;

public interface IJwtTokenGenerator
{
    (string accessToken, DateTime expiresAt) GenerateAccessToken(Usuarios usuario, IEnumerable<string> roles, IEnumerable<string> permisos);
    string GenerateRefreshToken();
    string HashToken(string token);
}
