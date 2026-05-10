using MediatR;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Interfaces.REST.Resources;
using GrotixBackend.Profiles.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

/// <summary>Operaciones de administrador sobre usuarios del perfil (no staff).</summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "admin")]
public sealed class AdminUsersController(IUserCommandService userCommandService, IMediator mediator) : ControllerBase
{
    public record AdminPatchUserRequest(
        string? Name,
        string? TaxId,
        string? Phone,
        string? ProfilePicture,
        bool? IsActive);

    public record AdminCreateUserRequest(
        string Email,
        string Password,
        int RoleId,
        int? AssociationId,
        string? Name = null,
        string? TaxId = null,
        string? Phone = null,
        bool IsActive = true);

    /// <summary>Crea credenciales y perfil con cualquier rol válido (sin invitación).</summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
    {
        try
        {
            var result = await mediator.Send(new AdminRegisterUserCommand(
                request.Email.Trim(),
                request.Password,
                request.RoleId,
                request.AssociationId,
                request.Name,
                request.TaxId,
                request.Phone,
                request.IsActive));
            return CreatedAtAction(nameof(PatchUser), new { userId = result.UserId },
                new { message = "Usuario creado.", identityId = result.IdentityId, userId = result.UserId });
        }
        catch (ApplicationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Actualización parcial de datos de perfil y estado de cuenta. <c>null</c> en un campo = sin cambio.</summary>
    [HttpPatch("{userId:int}")]
    public async Task<ActionResult<UserResource>> PatchUser(
        int userId,
        [FromBody] AdminPatchUserRequest request)
    {
        try
        {
            var updated = await userCommandService.Handle(new AdminPatchUserCommand(
                userId,
                request.Name,
                request.TaxId,
                request.Phone,
                request.ProfilePicture,
                request.IsActive));
            return UserResourceFromEntityAssembler.ToResourceFromEntity(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
