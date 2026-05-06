// Profiles/Interfaces/REST/Resources/UserResource.cs
namespace GrotixBackend.Profiles.Interfaces.REST.Resources;

public record UserPreferencesResource(bool Push, bool Email);

public record UserResource(
    int Id,
    int IdentityId,
    string? Name,
    string Email,
    string? TaxId,
    string? Phone,
    int RoleId,
    int? AssociationId,
    string? ProfilePicture,
    UserPreferencesResource? Preferences,
    DateTime CreatedAt,
    DateTime UpdatedAt
);