namespace GrotixBackend.Profiles.Domain.Model.Commands;

public sealed record CreateAssociationInviteCommand(
    int AssociationId,
    string InviteEmail,
    int RoleId,
    DateTime? ExpiresAt,
    int? CreatedByUserId);

/// <summary>El token en claro solo se devuelve al crear la invitación.</summary>
public sealed record CreateAssociationInviteResult(int InviteId, string PlaintextToken, DateTime? ExpiresAt);
