namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

/// <summary>Invitación de unión a una organización (token opaco; en BD solo hash).</summary>
public sealed class AssociationInvite
{
    public int Id { get; private set; }
    public int AssociationId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    /// <summary>Rol asignado al registrarse: <c>user_basic</c> (4) o <c>user_advanced</c> (5).</summary>
    public int RoleId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int? CreatedByUserId { get; private set; }

    private AssociationInvite() { }

    public AssociationInvite(
        int associationId,
        string tokenHash,
        int roleId,
        DateTime? expiresAt,
        int? createdByUserId)
    {
        if (associationId <= 0)
            throw new ArgumentException("AssociationId inválido.");
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("TokenHash requerido.");
        AssociationId = associationId;
        TokenHash = tokenHash.Trim();
        RoleId = roleId;
        ExpiresAt = expiresAt;
        CreatedByUserId = createdByUserId;
    }
}
