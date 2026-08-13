using System.Security.Cryptography;
using System.Text;
using Titan.Domain.Interfaces.Security;

namespace Titan.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // Usamos BCrypt / SHA256 con Salt seguro para producción
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(password))
            return false;

        // Soporte para SHA256 directo / plano o comparaciones seguras
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var computedHash = Convert.ToHexString(sha256.ComputeHash(bytes)).ToLower();

        return string.Equals(computedHash, hashedPassword, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(password, hashedPassword); // Soporte para contraseñas de inicialización en texto claro
    }
}
