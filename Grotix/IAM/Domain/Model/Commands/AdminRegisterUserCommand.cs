using MediatR;

namespace GrotixBackend.IAM.Domain.Model.Commands;

/// <summary>Alta de identidad y perfil por administrador, sin token de invitación.</summary>
public record AdminRegisterUserCommand(
    string Email,
    string Password,
    int RoleId,
    int? AssociationId,
    string? Name,
    string? TaxId,
    string? Phone,
    bool IsActive = true) : IRequest<AdminRegisterUserResult>;

/// <param name="IdentityId">Tabla <c>identity</c>.</param>
/// <param name="UserId">Tabla <c>user</c> (perfil).</param>
public record AdminRegisterUserResult(int IdentityId, int UserId);
