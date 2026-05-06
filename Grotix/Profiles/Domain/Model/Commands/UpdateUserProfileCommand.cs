namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record UpdateUserProfileCommand(
    int UserId,
    string? Name,
    string? TaxId,
    string? Phone,
    string? ProfilePicture
);
