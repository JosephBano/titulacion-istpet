using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Interfaces.Security;

namespace TitulacionIstpet.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string accessToken, DateTime expiresAt) GenerateAccessToken(Usuarios usuario, IEnumerable<string> roles, IEnumerable<string> permisos)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "TitulacionIstpetSystemSecretKeyForJwtAuthenticationSuperSecure2026!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "TitulacionIstpetApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "TitulacionIstpetApp";
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.Nombre ?? string.Empty),
            new(ClaimTypes.Email, usuario.EmailInstitucional ?? string.Empty),
            new("idSigafi", usuario.IdSigafi ?? string.Empty),
            new("tablaSigafi", usuario.TablaSigafi ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Optimización de tamaño de JWT Header: Incluir permisos distintos hasta un tope razonable
        var permisosUnicos = permisos.Distinct().Take(100);
        foreach (var permiso in permisosUnicos)
        {
            claims.Add(new Claim("permission", permiso));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
