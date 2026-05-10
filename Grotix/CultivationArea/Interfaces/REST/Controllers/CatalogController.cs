using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Controllers;

/// <summary>Catálogo maestro de cultivos (Staff/Admin para alta).</summary>
[ApiController]
[Route("api/v1/catalog")]
[Authorize]
public class CatalogController(
    ICropQueryService cropQueryService,
    ICropCommandService cropCommandService) : ControllerBase
{
    [HttpGet("crops")]
    public async Task<IActionResult> ListCrops()
    {
        var crops = await cropQueryService.ListAllAsync();
        return Ok(crops.Select(CultivationAreaResourceAssembler.ToCropResource).ToList());
    }

    [HttpGet("crops/{cropId:int}")]
    public async Task<IActionResult> GetCrop(int cropId)
    {
        var crop = await cropQueryService.Handle(new GetCropByIdQuery(cropId));
        if (crop == null) return NotFound();
        return Ok(CultivationAreaResourceAssembler.ToCropResource(crop));
    }

    public record CreateCropRequest(
        string CommonName,
        string ScientificName,
        double OptimalTemperature,
        double OptimalHumidity,
        double OptimalLight,
        int MaxStressTime,
        string? ImageUrl = null);

    [HttpPost("crops")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> CreateCrop([FromBody] CreateCropRequest request)
    {
        try
        {
            var crop = await cropCommandService.Handle(new CreateCropCommand(
                request.CommonName,
                request.ScientificName,
                request.OptimalTemperature,
                request.OptimalHumidity,
                request.OptimalLight,
                request.MaxStressTime,
                request.ImageUrl));
            return CreatedAtAction(nameof(GetCrop), new { cropId = crop.Id },
                CultivationAreaResourceAssembler.ToCropResource(crop));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
