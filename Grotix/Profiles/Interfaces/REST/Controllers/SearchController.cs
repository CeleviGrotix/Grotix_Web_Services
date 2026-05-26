using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/search")]
[Authorize] // Requiere token válido
public class SearchController(IMediator mediator, IUserQueryService userQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GlobalSearch([FromQuery] string? q = "")
    {
        // 1. Validar identidad del que llama
        var identityId = User.GetIdentityId();
        if (identityId == null) return Unauthorized();

        // 2. Obtener el perfil del usuario para saber su AssociationId
        var user = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (user == null) return Unauthorized();

        // 3. Lógica de visibilidad:
        // Si es admin o staff, busca en todo. 
        // Si no, solo busca dentro de su propia asociación.
        int? filterAssocId = (User.IsInRole("admin") || User.IsInRole("staff")) 
            ? null 
            : user.AssociationId;

        // 4. Ejecutar la búsqueda mediante MediatR
        var query = new GetGlobalSearchQuery(q, filterAssocId);
        var results = await mediator.Send(query);

        return Ok(results);
    }
}