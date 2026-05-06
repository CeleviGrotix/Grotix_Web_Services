using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/roles")]
public class RolesController(IRoleQueryService roleQueryService) : ControllerBase
{
    /// <summary>Catálogo de roles (semilla en BD). Útil para UI y asignación.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var roles = await roleQueryService.GetAllAsync();
        var resources = roles.Select(r => new RoleResource(r.Id, r.Name, r.Description)).ToList();
        return Ok(resources);
    }
}
