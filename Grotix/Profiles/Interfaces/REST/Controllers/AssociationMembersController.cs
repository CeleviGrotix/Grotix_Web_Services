using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

/// <summary>Miembros activos de una asociación (roles <c>user_*</c>).</summary>
[ApiController]
[Route("api/v1/associations/{associationId:int}/members")]
[Authorize]
public sealed class AssociationMembersController(
    IUserQueryService userQueryService,
    IRoleQueryService roleQueryService,
    IAssociationExistenceService associationExistenceService) : ControllerBase
{
    /// <summary>
    /// Lista miembros de la organización. Permitido: <c>admin</c>, <c>staff</c>,
    /// o <c>user_admin</c> de la misma asociación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListMembers(
        int associationId,
        CancellationToken cancellationToken = default)
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

        if (!await associationExistenceService.ExistsAsync(associationId, cancellationToken))
            return NotFound();

        var members = await userQueryService.Handle(new GetFarmersByAssociationQuery(associationId));
        var roles = (await roleQueryService.GetAllAsync()).ToDictionary(r => r.Id, r => r.Name);

        return Ok(members
            .Where(m => m.IsActive)
            .Select(m => new
            {
                userId = m.Id,
                name = m.Name,
                email = m.Email.Value,
                roleId = m.RoleId,
                roleName = roles.TryGetValue(m.RoleId, out var roleName) ? roleName : "unknown",
                profilePicture = m.ProfilePicture
            })
            .ToList());
    }
}
