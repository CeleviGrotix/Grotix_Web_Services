using GrotixBackend.Profiles.Domain.Services;

namespace GrotixBackend.Profiles.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public bool Verify(string plainPassword, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(passwordHash))
            return false;
        return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
    }
}
