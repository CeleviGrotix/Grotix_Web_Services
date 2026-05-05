// Profiles/Interfaces/REST/Resources/UserResource.cs
namespace GrotixBackend.Profiles.Interfaces.REST.Resources;

public record UserResource(
    int Id,
    string? Name,
    string Email,
    string? TaxId,
    string? Phone,
    int RoleId
);