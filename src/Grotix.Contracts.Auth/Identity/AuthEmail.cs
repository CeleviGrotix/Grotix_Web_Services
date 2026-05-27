using System.Net.Mail;

namespace GrotixBackend.Contracts.Auth.Identity;

public static class AuthEmail
{
    public static string Normalize(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo no puede estar vacío.");

        try
        {
            var trimmed = email.Trim();
            var address = new MailAddress(trimmed);
            if (address.Address != trimmed || !trimmed.Contains('@') || trimmed.Split('@')[1].Length < 2)
                throw new FormatException();

            return trimmed;
        }
        catch
        {
            throw new ArgumentException($"El correo '{email}' no tiene un formato válido.");
        }
    }
}
