using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

/// <summary>Directorio de agricultores (roles user_*); no incluye cuentas <c>admin</c> ni <c>staff</c>.</summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController(IUserQueryService userQueryService) : ControllerBase
{
    /// <summary>
    /// Lista solo agricultores (<c>user_admin</c>, <c>user_basic</c>, <c>user_advanced</c>).
    /// <c>admin</c> y <c>staff</c> ven todos; <c>user_admin</c> solo los de su misma asociación.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null)
            return Unauthorized();

        var caller = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (caller == null)
            return Unauthorized();

        if (User.IsInRole("admin") || User.IsInRole("staff"))
        {
            var users = await userQueryService.Handle(new GetAllFarmersQuery());
            return Ok(users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity).ToList());
        }

        if (User.IsInRole("user_admin"))
        {
            if (caller.AssociationId is not { } assocId)
                return Forbid();

            var users = await userQueryService.Handle(new GetFarmersByAssociationQuery(assocId));
            return Ok(users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity).ToList());
        }

        return Forbid();
    }

    /// <summary>Obtiene un agricultor por id; <c>404</c> si no existe o si es cuenta sistema (admin/staff).</summary>
    [HttpGet("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int userId)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null)
            return Unauthorized();

        var caller = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (caller == null)
            return Unauthorized();

        var farmer = await userQueryService.Handle(new GetFarmerByIdQuery(userId));
        if (farmer == null)
            return NotFound();

        if (User.IsInRole("admin") || User.IsInRole("staff"))
            return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(farmer));

        if (User.IsInRole("user_admin"))
        {
            if (caller.AssociationId is not { } assocId || farmer.AssociationId != assocId)
                return Forbid();
            return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(farmer));
        }

        return Forbid();
    }
}
