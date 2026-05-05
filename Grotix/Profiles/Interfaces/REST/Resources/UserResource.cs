namespace GrotixBackend.Profiles.Interfaces.REST.Resources;

public record UserResource(
    int Id,
    string Name,
    string Email,
    string TaxId,
    string ProfilePicture
);