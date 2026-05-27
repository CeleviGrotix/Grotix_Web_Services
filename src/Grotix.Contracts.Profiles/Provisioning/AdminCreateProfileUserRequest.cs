namespace GrotixBackend.Contracts.Profiles.Provisioning;

public sealed record AdminCreateProfileUserRequest(
    int IdentityId,
    string Email,
    int RoleId,
    int? AssociationId,
    string? Name,
    string? TaxId,
    string? Phone,
    bool IsActive = true);
