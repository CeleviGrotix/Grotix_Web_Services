using System;
using BCrypt.Net;

namespace GrotixBackend.IAM.Domain.Model.ValueObjects
{
    /// <summary>
    /// Value Object que encapsula la lógica de hashing y verificación de contraseñas.
    /// </summary>
    public class PasswordHash : IEquatable<PasswordHash>
    {
        public string HashedValue { get; private set; }

        private PasswordHash() { }

        /// <summary>
        /// Crea un nuevo hash a partir de una contraseña en texto plano.
        /// </summary>
        public PasswordHash(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(plainTextPassword));
            }
            // Generamos el hash usando BCrypt
            HashedValue = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        }

        /// <summary>
        /// Compara una contraseña en texto plano con el hash almacenado.
        /// </summary>
        public bool Matches(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(HashedValue))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(plainTextPassword, HashedValue);
        }

        #region Equality Logic

        public bool Equals(PasswordHash other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return HashedValue == other.HashedValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PasswordHash other && Equals(other);
        }

        public override int GetHashCode() => HashedValue?.GetHashCode() ?? 0;

        public static bool operator ==(PasswordHash left, PasswordHash right) => Equals(left, right);
        public static bool operator !=(PasswordHash left, PasswordHash right) => !Equals(left, right);

        #endregion

        public override string ToString() => "PasswordHash [PROTECTED]";
    }
}