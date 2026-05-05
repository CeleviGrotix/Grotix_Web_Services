namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record CreateUserCommand(
    int IdentityId,
    string Name,
    string Email,
    string TaxId,
    string Phone
);