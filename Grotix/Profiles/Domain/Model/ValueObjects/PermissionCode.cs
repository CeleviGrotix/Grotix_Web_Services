namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>VO del informe Profile: código de permiso de acción (p. ej. irrigation:start).</summary>
public sealed class PermissionCode : IEquatable<PermissionCode>
{
    public string Value { get; }

    private PermissionCode(string value) => Value = value;

    public static PermissionCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código de permiso no puede estar vacío.");
        var trimmed = code.Trim().ToLowerInvariant();
        if (trimmed.Length > 64 || trimmed.Contains(' ', StringComparison.Ordinal))
            throw new ArgumentException("El código de permiso tiene un formato inválido.");
        return new PermissionCode(trimmed);
    }

    public bool Equals(PermissionCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PermissionCode c && Equals(c);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;

    public static implicit operator string(PermissionCode c) => c.Value;
}
