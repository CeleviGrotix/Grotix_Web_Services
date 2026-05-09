using System.Security.Cryptography;
using System.Text;

namespace GrotixBackend.Profiles.Domain.Security;

/// <summary>Generación de tokens opacos y hash SHA-256 para persistencia.</summary>
public static class InviteTokenHasher
{
    /// <summary>Token aleatorio URL-safe (mostrar una sola vez al crear invitación).</summary>
    public static string GenerateToken()
    {
        Span<byte> buf = stackalloc byte[32];
        RandomNumberGenerator.Fill(buf);
        return Convert.ToBase64String(buf).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Hash hexadecimal en mayúsculas (coincide con columnas comparadas en SQL).</summary>
    public static string Hash(string plaintextToken)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintextToken.Trim());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
