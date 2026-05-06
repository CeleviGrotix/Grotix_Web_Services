using System.Text.RegularExpressions;

namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>VO del informe Profile: formato internacional básico (E.164 simplificado).</summary>
public sealed class UserPhone : IEquatable<UserPhone>
{
    private static readonly Regex Pattern = new(@"^\+?[0-9]{8,15}$", RegexOptions.Compiled);

    public string Value { get; }

    private UserPhone(string value) => Value = value;

    public static UserPhone Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("El teléfono no puede estar vacío.");
        var digits = raw.Trim().Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal);
        if (!Pattern.IsMatch(digits))
            throw new ArgumentException("El teléfono debe usar formato internacional (ej. +519998887766).");
        return new UserPhone(digits);
    }

    public bool Equals(UserPhone? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is UserPhone p && Equals(p);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;

    public static implicit operator string(UserPhone p) => p.Value;
}
