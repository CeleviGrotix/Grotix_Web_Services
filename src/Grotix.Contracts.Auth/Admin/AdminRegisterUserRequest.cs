namespace GrotixBackend.Contracts.Auth.Admin;

public sealed record AdminRegisterUserRequest(
    string Email,
    string Password,
    int RoleId,
    int? AssociationId,
    string? Name,
    string? TaxId,
    string? Phone,
    bool IsActive = true);
