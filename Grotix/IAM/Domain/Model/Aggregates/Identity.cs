namespace GrotixBackend.IAM.Domain.Model.Aggregates;

using GrotixBackend.IAM.Domain.Model.ValueObjects;
using System.Text.RegularExpressions;

public class Identity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string UserName { get; private set; }
    public PasswordHash HashedPassword { get; private set; }

    protected Identity() { }

    public Identity(int userId, string email, string passwordHashValue)
    {
        ValidateEmail(email);

        UserId = userId;
        UserName = email;

        this.HashedPassword = PasswordHash.FromHash(passwordHashValue);
    }

    public void UpdateCredentials(string email, string passwordHashValue)
    {
        ValidateEmail(email);
        UserName = email;
        HashedPassword = PasswordHash.FromHash(passwordHashValue);
    }

    private void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo no puede estar vacío.");

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email || !email.Contains(".") || email.Split('@')[1].Length < 3)
                throw new Exception();
        }
        catch
        {
            throw new ArgumentException($"El correo '{email}' no tiene un formato válido (ejemplo@dominio.com).");
        }
    }

    public bool VerifyPassword(string plainTextPassword)
    {
        return HashedPassword != null && HashedPassword.Matches(plainTextPassword);
    }
    public static void VerifyPasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            throw new ArgumentException("La contraseña debe tener al menos una letra mayúscula.");

        if (!Regex.IsMatch(password, @"\d"))
            throw new ArgumentException("La contraseña debe tener al menos un número.");

        if (!Regex.IsMatch(password, @"[@$!%*?&]"))
            throw new ArgumentException("La contraseña debe tener al menos un carácter especial (@$!%*?&).");
    }

}