using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

/// <summary>Informe Profile: registro de asociaciones agrarias (Staff/Admin).</summary>
[ApiController]
[Route("api/v1/associations")]
[Authorize(Roles = "admin,staff")]
public class AssociationController(
    IAssociationRepository associationRepository,
    IUserQueryService userQueryService,
    IProfilesUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await associationRepository.ListAsync();
        return Ok(list.Select(a => new AssociationResource(a.Id, a.Name, a.ContactEmail.Value)).ToList());
    }

    [HttpGet("{associationId:int}")]
    public async Task<IActionResult> GetById(int associationId)
    {
        var entity = await associationRepository.GetByIdAsync(associationId);
        if (entity == null) return NotFound();
        return Ok(new AssociationResource(entity.Id, entity.Name, entity.ContactEmail.Value));
    }

    /// <summary>Devuelve la asociación a la que pertenece el usuario autenticado.</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return Unauthorized();

        var user = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (user?.AssociationId == null)
            return NotFound(new { message = "No perteneces a ninguna asociación." });

        var entity = await associationRepository.GetByIdAsync(user.AssociationId.Value);
        if (entity == null) return NotFound();

        return Ok(new AssociationResource(entity.Id, entity.Name, entity.ContactEmail.Value));
    }

    public record CreateAssociationRequest(string Name, string Email);

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Create([FromBody] CreateAssociationRequest request)
    {
        var email = UserEmail.Create(request.Email);
        var entity = new Association(request.Name, email);
        await associationRepository.AddAsync(entity);
        await unitOfWork.CompleteAsync();
        return Created($"/api/v1/associations/{entity.Id}",
            new AssociationResource(entity.Id, entity.Name, entity.ContactEmail.Value));
    }

    /// <summary>Solo <c>admin</c>. Actualización parcial: omitir propiedad = mantener valor actual.</summary>
    public record PatchAssociationRequest(string? Name, string? Email);

    [HttpPatch("{associationId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Patch(int associationId, [FromBody] PatchAssociationRequest request)
    {
        var entity = await associationRepository.GetByIdAsync(associationId);
        if (entity == null)
            return NotFound();

        try
        {
            var name = request.Name ?? entity.Name;
            var email = request.Email != null ? UserEmail.Create(request.Email) : entity.ContactEmail;
            entity.Update(name, email);
            await unitOfWork.CompleteAsync();
            return Ok(new AssociationResource(entity.Id, entity.Name, entity.ContactEmail.Value));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public record AssociationResource(int Id, string Name, string Email);
}
