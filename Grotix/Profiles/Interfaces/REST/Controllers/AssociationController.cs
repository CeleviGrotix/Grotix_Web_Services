using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

/// <summary>Informe Profile: registro de asociaciones agrarias (Staff/Admin).</summary>
[ApiController]
[Route("api/v1/associations")]
[Authorize(Roles = "admin,staff")]
public class AssociationController(
    IAssociationRepository associationRepository,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await associationRepository.ListAsync();
        return Ok(list.Select(a => new AssociationResource(a.Id, a.Name, a.ContactEmail.Value)).ToList());
    }

    public record CreateAssociationRequest(string Name, string Email);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssociationRequest request)
    {
        var email = UserEmail.Create(request.Email);
        var entity = new Association(request.Name, email);
        await associationRepository.AddAsync(entity);
        await unitOfWork.CompleteAsync();
        return Created($"/api/v1/associations/{entity.Id}",
            new AssociationResource(entity.Id, entity.Name, entity.ContactEmail.Value));
    }

    public record AssociationResource(int Id, string Name, string Email);
}
