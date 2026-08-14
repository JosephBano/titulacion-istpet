using System.Security.Cryptography;
using System.Text;
using TitulacionIstpet.Application.Auth;
using TitulacionIstpet.Domain.Interfaces.Security;

namespace TitulacionIstpet.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly IVerificadorCredenciales _verificador;

    public PasswordHasher(IVerificadorCredenciales verificador)
    {
        _verificador = verificador;
    }

    public string HashPassword(string password)
    {
        return _verificador.Hashear(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var resultado = _verificador.Verificar(password, hashedPassword);
        if (resultado.EsValida)
        {
            return true;
        }

        // Soporte de compatibilidad SHA256 histórico
        var bytes = Encoding.UTF8.GetBytes(password);
        var computedHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLower();

        return string.Equals(computedHash, hashedPassword, StringComparison.OrdinalIgnoreCase);
    }
}
