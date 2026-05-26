namespace GrotixBackend.Contracts.Profiles.Access;

public sealed record UserAccessContext(
    int UserId,
    int IdentityId,
    int? AssociationId,
    bool IsActive);
