// Profiles/Domain/Model/Commands/CreateUserCommand.cs
namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record CreateUserCommand(
    int IdentityId,
    string Email,
    int RoleId = 4,
    string? Name = null,
    string? TaxId = null,
    string? Phone = null,
    int? AssociationId = null,
    bool IsActive = true
);