namespace GrotixBackend.Contracts.Profiles.Provisioning;

public sealed record CreateProfileUserRequest(
    int IdentityId,
    string Email,
    int RoleId = 4,
    int? AssociationId = null);
