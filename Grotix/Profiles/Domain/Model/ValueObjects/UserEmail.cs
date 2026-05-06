namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>VO del informe Profile: valida formato de correo antes de crear/modificar cuenta.</summary>
public sealed class UserEmail : IEquatable<UserEmail>
{
    public string Value { get; }

    private UserEmail(string value) => Value = value;

    public static UserEmail Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo no puede estar vacío.");
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email || !email.Contains('@') || email.Split('@')[1].Length < 2)
                throw new FormatException();
        }
        catch
        {
            throw new ArgumentException($"El correo '{email}' no tiene un formato válido.");
        }

        return new UserEmail(email.Trim());
    }

    public bool Equals(UserEmail? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is UserEmail e && Equals(e);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;

    public static implicit operator string(UserEmail e) => e.Value;
}
