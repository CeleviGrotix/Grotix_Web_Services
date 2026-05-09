using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Interfaces.REST.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

/// <summary>Invitaciones para registrar agricultores (<c>user_basic</c> / <c>user_advanced</c>) en una organización.</summary>
[ApiController]
[Route("api/v1/associations/{associationId:int}/invites")]
[Authorize]
public sealed class AssociationInvitesController(
    IUserQueryService userQueryService,
    IAssociationInviteCommandService inviteCommandService) : ControllerBase
{
    public record CreateInviteRequest(int RoleId, DateTime? ExpiresAt);

    public record CreateInviteResponse(int InviteId, string Token, DateTime? ExpiresAt);

    /// <summary>
    /// Genera un token de invitación (mostrar/guardar una sola vez).
    /// Permitido: <c>admin</c>, <c>staff</c>, o <c>user_admin</c> de la misma asociación.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateInvite(int associationId, [FromBody] CreateInviteRequest request)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null)
            return Unauthorized();

        var caller = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (caller == null)
            return Unauthorized();

        var allowed =
            User.IsInRole("admin") ||
            User.IsInRole("staff") ||
            (User.IsInRole("user_admin") && caller.AssociationId == associationId);

        if (!allowed)
            return Forbid();

        try
        {
            var result = await inviteCommandService.Handle(new CreateAssociationInviteCommand(
                associationId,
                request.RoleId,
                request.ExpiresAt,
                caller.Id));

            return Ok(new CreateInviteResponse(result.InviteId, result.PlaintextToken, result.ExpiresAt));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
