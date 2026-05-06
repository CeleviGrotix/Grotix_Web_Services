using System;
using System.Collections.Generic;

namespace GrotixBackend.IAM.Domain.Model.ValueObjects;

/// <summary>VO de credencial: solo almacena el hash persistido (el hashing lo hace <see cref="GrotixBackend.Profiles.Domain.Services.IPasswordHasher"/>).</summary>
public class PasswordHash : IEquatable<PasswordHash>
{
    public string HashedValue { get; private set; } = null!;

    private PasswordHash() { }

    public static PasswordHash FromHash(string existingHash)
    {
        if (string.IsNullOrWhiteSpace(existingHash))
            throw new ArgumentException("El hash no puede estar vacío.", nameof(existingHash));
        return new PasswordHash { HashedValue = existingHash };
    }

    public bool Equals(PasswordHash? other) =>
        other is not null && HashedValue == other.HashedValue;

    public override bool Equals(object? obj) => obj is PasswordHash other && Equals(other);
    public override int GetHashCode() => HashedValue.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(PasswordHash? left, PasswordHash? right) => Equals(left, right);
    public static bool operator !=(PasswordHash? left, PasswordHash? right) => !Equals(left, right);

    public override string ToString() => "PasswordHash [PROTECTED]";
}
