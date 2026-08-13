using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Titan.Domain.Entities;
using Titan.Domain.Interfaces.Security;

namespace Titan.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string accessToken, DateTime expiresAt) GenerateAccessToken(usuarios usuario, IEnumerable<string> roles, IEnumerable<string> permisos)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "TitanSystemSecretKeyForJwtAuthenticationSuperSecure2026!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "TitanApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "TitanApp";
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.idUsuario.ToString()),
            new(ClaimTypes.Name, usuario.nombre ?? string.Empty),
            new(ClaimTypes.Email, usuario.emailInstitucional ?? string.Empty),
            new("idSigafi", usuario.idSigafi ?? string.Empty),
            new("tablaSigafi", usuario.tablaSigafi ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Optimización de tamaño de JWT Header: Incluir permisos distintos hasta un tope razonable para evitar HTTP 431
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
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
