namespace GrotixBackend.Contracts.Profiles.Invites;

public sealed record ValidatedAssociationInvite(
    int InviteId,
    int AssociationId,
    int RoleId,
    string InviteEmail);
