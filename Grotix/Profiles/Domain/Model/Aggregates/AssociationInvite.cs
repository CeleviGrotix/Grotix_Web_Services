namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

/// <summary>Invitación de unión a una organización (token opaco; en BD solo hash).</summary>
public sealed class AssociationInvite
{
    public int Id { get; private set; }
    public int AssociationId { get; private set; }
    /// <summary>Correo normalizado al que pertenece la invitación; el registro debe usar el mismo.</summary>
    public string InviteEmail { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    /// <summary>Rol asignado al registrarse (p. ej. <c>user_admin</c>, <c>user_basic</c>, <c>user_advanced</c>).</summary>
    public int RoleId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int? CreatedByUserId { get; private set; }

    private AssociationInvite() { }

    public AssociationInvite(
        int associationId,
        string inviteEmail,
        string tokenHash,
        int roleId,
        DateTime? expiresAt,
        int? createdByUserId)
    {
        if (associationId <= 0)
            throw new ArgumentException("AssociationId inválido.");
        if (string.IsNullOrWhiteSpace(inviteEmail))
            throw new ArgumentException("InviteEmail requerido.");
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("TokenHash requerido.");
        AssociationId = associationId;
        InviteEmail = inviteEmail.Trim();
        TokenHash = tokenHash.Trim();
        RoleId = roleId;
        ExpiresAt = expiresAt;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }
}
