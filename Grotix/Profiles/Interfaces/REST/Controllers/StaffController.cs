using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/staff")]
[Authorize(Roles = "admin")]
public class StaffController(
    IStaffCommandService staffCommandService,
    IStaffQueryService staffQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await staffQueryService.GetAllAsync();
        return Ok(list.Select(ToResource).ToList());
    }

    [HttpGet("{staffId:int}")]
    public async Task<IActionResult> GetById(int staffId)
    {
        var staff = await staffQueryService.GetByIdAsync(staffId);
        if (staff == null) return NotFound();
        return Ok(ToResource(staff));
    }

    public record CreateStaffRequest(int UserId, TechnicalRole TechnicalRole, DateTime? LastSystemAccess);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request)
    {
        try
        {
            var staff = await staffCommandService.Handle(
                new CreateStaffCommand(request.UserId, request.TechnicalRole, request.LastSystemAccess));
            return CreatedAtAction(nameof(GetById), new { staffId = staff.Id }, ToResource(staff));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    public record UpdateStaffRequest(TechnicalRole? TechnicalRole, DateTime? LastSystemAccess, bool? IsActive);

    [HttpPatch("{staffId:int}")]
    public async Task<IActionResult> Patch(int staffId, [FromBody] UpdateStaffRequest request)
    {
        try
        {
            var staff = await staffCommandService.Handle(
                new UpdateStaffCommand(staffId, request.TechnicalRole, request.LastSystemAccess, request.IsActive));
            return Ok(ToResource(staff));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static StaffResource ToResource(Staff s) =>
        new(s.Id, s.UserId, s.TechnicalRole.ToString(), s.LastSystemAccess, s.IsActive);
}
