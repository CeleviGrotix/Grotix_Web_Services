namespace GrotixBackend.IAM.Domain.Model.Aggregates;

using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using System.Text.RegularExpressions;

/// <summary>
/// Entidad de credenciales del informe Profile (ubicación técnica IAM hasta fusionar carpetas).
/// </summary>
public class Identity
{
    public int Id { get; private set; }
    public string UserName { get; private set; } = null!;
    public PasswordHash HashedPassword { get; private set; } = null!;

    protected Identity() { }

    /// <summary>Crea identidad con correo validado y hash ya calculado por el servicio de aplicación.</summary>
    public Identity(string email, PasswordHash passwordHash)
    {
        var vo = UserEmail.Create(email);
        UserName = vo.Value;
        HashedPassword = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
    }

    public static void VerifyPasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
        if (!Regex.IsMatch(password, @"[A-Z]"))
            throw new ArgumentException("La contraseña debe tener al menos una mayúscula.");
        if (!Regex.IsMatch(password, @"\d"))
            throw new ArgumentException("La contraseña debe tener al menos un número.");
        if (!Regex.IsMatch(password, @"[@$!%*?&]"))
            throw new ArgumentException("La contraseña debe tener al menos un carácter especial.");
    }
}
