using System.Text.RegularExpressions;

namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>VO del informe Profile: código de permiso en formato UPPER_SNAKE (coincide con BD y JWT).</summary>
public sealed class PermissionCode : IEquatable<PermissionCode>
{
    private static readonly Regex ValidPattern = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);

    public string Value { get; }

    private PermissionCode(string value) => Value = value;

    public static PermissionCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código de permiso no puede estar vacío.");
        var trimmed = code.Trim();
        if (trimmed.Length > 64 || !ValidPattern.IsMatch(trimmed))
            throw new ArgumentException("El código de permiso tiene un formato inválido (use UPPER_SNAKE).");
        return new PermissionCode(trimmed.ToUpperInvariant());
    }

    public bool Equals(PermissionCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PermissionCode c && Equals(c);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;

    public static implicit operator string(PermissionCode c) => c.Value;
}
