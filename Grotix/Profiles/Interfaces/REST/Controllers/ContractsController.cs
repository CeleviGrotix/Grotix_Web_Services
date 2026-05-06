using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/contracts")]
[Authorize(Roles = "Admin,Staff")]
public sealed class ContractsController(
    IContractQueryService contractQueryService,
    IContractCommandService contractCommandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await contractQueryService.ListAllAsync();
        return Ok(items.Select(ToResource).ToList());
    }

    [HttpGet("{contractId:int}")]
    public async Task<IActionResult> GetById(int contractId)
    {
        var contract = await contractQueryService.Handle(new GetContractByIdQuery(contractId));
        if (contract == null) return NotFound();
        return Ok(ToResource(contract));
    }

    public record CreateContractRequest(
        int AssociationId,
        DateTime StartDate,
        DateTime EndDate,
        ContractStatus Status,
        int MaxZones,
        int MaxMicrocontrollers,
        float TotalAmount,
        ContractCurrency Currency,
        ContractPaymentFrequency PaymentFrequency,
        bool IsSuspended);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request)
    {
        try
        {
            var contract = await contractCommandService.Handle(new CreateContractCommand(
                request.AssociationId,
                request.StartDate,
                request.EndDate,
                request.Status,
                request.MaxZones,
                request.MaxMicrocontrollers,
                request.TotalAmount,
                request.Currency,
                request.PaymentFrequency,
                request.IsSuspended));

            return CreatedAtAction(nameof(GetById), new { contractId = contract.Id }, ToResource(contract));
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
