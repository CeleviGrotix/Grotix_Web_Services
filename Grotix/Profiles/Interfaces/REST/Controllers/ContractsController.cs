using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Interfaces.REST.Auth;
using GrotixBackend.Profiles.Interfaces.REST.Requests;
using GrotixBackend.Profiles.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

/// <summary>Contratos comerciales por asociación. Alta solo <c>admin</c>/<c>staff</c>; lectura también <c>user_admin</c> de la misma organización.</summary>
[ApiController]
[Route("api/v1/contracts")]
[Authorize]
public sealed class ContractsController(
    IContractQueryService contractQueryService,
    IContractCommandService contractCommandService,
    IUserQueryService userQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null)
            return Unauthorized();

        var caller = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (caller == null)
            return Unauthorized();

        IReadOnlyList<Contract> items;
        if (User.IsInRole("admin") || User.IsInRole("staff"))
            items = await contractQueryService.ListAllAsync();
        else if (User.IsInRole("user_admin"))
        {
            if (caller.AssociationId is not { } assocId)
                return Forbid();
            items = await contractQueryService.ListByAssociationAsync(assocId);
        }
        else
            return Forbid();

        return Ok(items.Select(ToResource).ToList());
    }

    [HttpGet("{contractId:int}")]
    public async Task<IActionResult> GetById(int contractId)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null)
            return Unauthorized();

        var caller = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (caller == null)
            return Unauthorized();

        var contract = await contractQueryService.Handle(new GetContractByIdQuery(contractId));
        if (contract == null)
            return NotFound();

        if (User.IsInRole("admin") || User.IsInRole("staff"))
            return Ok(ToResource(contract));

        if (User.IsInRole("user_admin"))
        {
            if (caller.AssociationId != contract.AssociationId)
                return Forbid();
            return Ok(ToResource(contract));
        }

        return Forbid();
    }

    public record CreateContractResponse(
        ContractResource Contract,
        int OrganizationAdminUserId,
        string OrganizationAdminEmail);

    /// <summary>Crea el contrato y el usuario <c>user_admin</c> vinculado a la asociación (si aún no tiene uno).</summary>
    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request)
    {
        try
        {
            var result = await contractCommandService.Handle(new CreateContractCommand(
                request.AssociationId,
                request.StartDate,
                request.EndDate,
                request.Status,
                request.MaxZones,
                request.MaxMicrocontrollers,
                request.TotalAmount,
                request.Currency,
                request.PaymentFrequency,
                request.IsSuspended,
                request.OrgAdminEmail,
                request.OrgAdminPassword,
                request.OrgAdminName));

            var response = new CreateContractResponse(
                ToResource(result.Contract),
                result.OrganizationAdminUserId,
                request.OrgAdminEmail.Trim());

            return CreatedAtAction(nameof(GetById), new { contractId = result.Contract.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static ContractResource ToResource(Contract c) =>
        new(
            c.Id,
            c.AssociationId,
            c.StartDate,
            c.EndDate,
            c.Status.ToString(),
            c.MaxZones,
            c.MaxMicrocontrollers,
            c.TotalAmount,
            c.Currency.ToString(),
            c.PaymentFrequency.ToString(),
            c.IsSuspended);
}
