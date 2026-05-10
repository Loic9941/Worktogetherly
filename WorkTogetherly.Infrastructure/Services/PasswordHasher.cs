using System.Security.Cryptography;
using WorkTogetherly.Application.Services;

namespace WorkTogetherly.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return $"$pbkdf2-sha256$10000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}
