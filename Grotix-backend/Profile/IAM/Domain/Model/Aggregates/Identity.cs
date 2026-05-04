namespace GrotixBackend.IAM.Domain.Model.Aggregates;

using GrotixBackend.IAM.Domain.Model.ValueObjects;

/// <summary>
/// Agregado Identity: Controla el acceso y la autenticación en Grotix.
/// </summary>
public class Identity
{
    public int Id { get; private set; }

    /// <summary>
    /// Referencia al ID del usuario en el contexto de Profile.
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Nombre de usuario único para el login.
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// Contraseña hasheada (Value Object).
    /// </summary>
    public PasswordHash HashedPassword { get; private set; }

    protected Identity() { }

    /// <summary>
    /// Constructor para crear una nueva identidad en Grotix.
    /// </summary>
    public Identity(int userId, string username, string hashedPassword)
    {
        UserId = userId;
        UserName = username;
        HashedPassword = new PasswordHash(hashedPassword);
    }

    /// <summary>
    /// Actualiza las credenciales de acceso.
    /// </summary>
    public void UpdateCredentials(string username, string hashedPassword)
    {
        UserName = username;
        HashedPassword = new PasswordHash(hashedPassword);
    }

    /// <summary>
    /// Valida si la contraseña proporcionada coincide con el hash almacenado.
    /// </summary>
    public bool VerifyPassword(string plainTextPassword)
    {
        return HashedPassword != null && HashedPassword.Matches(plainTextPassword);
    }
}