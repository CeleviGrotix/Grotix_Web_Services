using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.Queries;
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
        string OrganizationAdminEmail,
        int? OrgAdminInviteId,
        string? OrgAdminInviteToken,
        int? AssignedOrgAdminUserId,
        bool InviteSkipped);

    /// <summary>Crea el contrato y una invitación para que el correo indicado se registre como <c>user_admin</c>.</summary>
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
                request.OrgAdminEmail));

            var response = new CreateContractResponse(
                ToResource(result.Contract),
                result.OrgAdminEmail.Trim(),
                result.OrgAdminInviteId,
                result.OrgAdminInvitePlaintextToken,
                result.AssignedOrgAdminUserId,
                result.InviteSkipped);

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

    public record UpdateContractRequest(
        DateTime? EndDate,
        ContractStatus? Status,
        int? MaxZones,
        int? MaxMicrocontrollers,
        float? TotalAmount,
        ContractCurrency? Currency,
        ContractPaymentFrequency? PaymentFrequency,
        bool? IsSuspended);

    /// <summary>Edita un contrato existente. Solo envía los campos que deseas cambiar.</summary>
    [HttpPatch("{contractId:int}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Update(int contractId, [FromBody] UpdateContractRequest request)
    {
        try
        {
            var command = new UpdateContractCommand(
                contractId,
                request.EndDate,
                request.Status,
                request.MaxZones,
                request.MaxMicrocontrollers,
                request.IsSuspended,
                request.TotalAmount,
                request.Currency,
                request.PaymentFrequency);

            var updatedContract = await contractCommandService.Handle(command);
            
            if (updatedContract == null) 
                return NotFound(new { message = "Contrato no encontrado." });

            return Ok(ToResource(updatedContract));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Elimina físicamente un contrato (Hard Delete).</summary>
    [HttpDelete("{contractId:int}")]
    [Authorize(Roles = "admin")] // Protegido solo para admins por seguridad
    public async Task<IActionResult> Delete(int contractId)
    {
        try
        {
            await contractCommandService.Handle(new DeleteContractCommand(contractId));
            return NoContent(); // 204 No Content es el éxito estándar para un Delete
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
