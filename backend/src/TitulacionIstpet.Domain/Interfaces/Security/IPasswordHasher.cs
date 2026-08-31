namespace TitulacionIstpet.Domain.Interfaces.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    bool NeedsRehash(string? hashedPassword);
}

