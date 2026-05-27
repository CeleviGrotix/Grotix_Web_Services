namespace GrotixBackend.Profiles.Domain.Model.Commands;

/// <summary>Actualización parcial de perfil por administrador (<c>null</c> = sin cambio).</summary>
public record AdminPatchUserCommand(
    int UserId,
    string? Name,
    string? TaxId,
    string? Phone,
    string? ProfilePicture,
    int? RoleId,
    int? AssociationId,
    bool? IsActive);
