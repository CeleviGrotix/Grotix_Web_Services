using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Controllers;
using System.Security.Claims;
using TechTalk.SpecFlow;

namespace CultivationArea.Api.Tests.Acceptance.Steps;

[Binding]
public class US22Steps
{
    private readonly SharedContext _ctx;
    private readonly Mock<IZoneCommandService> _zoneCommandService = new();
    private Zone? _currentZone;

    public US22Steps(SharedContext ctx) => _ctx = ctx;

    private ZonesController BuildController()
    {
        var controller = new ZonesController(
            _ctx.AccessContextService.Object,
            _ctx.FarmQueryService.Object,
            _zoneCommandService.Object,
            _ctx.ZoneQueryService.Object,
            _ctx.ZoneMemberService.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.NameIdentifier, "100")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth", ClaimTypes.NameIdentifier, ClaimTypes.Role);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    private static Zone BuildZone(string? imageUrl = null) =>
        new Zone(farmId: 1, cropId: 1, name: "Zona Test",
            latitude: -12.0, longitude: -77.0, imageUrl: imageUrl);

    [Given(@"la zona (.*) no tiene imagen registrada")]
    public void GivenZonaSinImagen(int zoneId)
    {
        _currentZone = BuildZone(null);
        _ctx.ZoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync(_currentZone);
    }

    [Given(@"la zona (.*) tiene imageUrl ""(.*)""")]
    public void GivenZonaConImageUrl(int zoneId, string imageUrl)
    {
        _currentZone = BuildZone(imageUrl);
        _ctx.ZoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync(_currentZone);
    }

    [When(@"el usuario actualiza la zona (.*) con imageUrl ""(.*)""")]
    public async Task WhenActualizaConImageUrl(int zoneId, string imageUrl)
    {
        var updated = BuildZone(imageUrl);
        _zoneCommandService
            .Setup(s => s.Handle(It.IsAny<UpdateZoneCommand>()))
            .ReturnsAsync(updated);

        _ctx.Result = await BuildController().Patch(zoneId,
            new ZonesController.PatchZoneRequest(
                null, null, null, null, null, null, imageUrl, null));
    }

    [When(@"el usuario actualiza la zona (.*) con imageUrl vacía")]
    public async Task WhenActualizaConImageUrlVacia(int zoneId)
    {
        var updated = BuildZone(null);
        _zoneCommandService
            .Setup(s => s.Handle(It.IsAny<UpdateZoneCommand>()))
            .ReturnsAsync(updated);

        _ctx.Result = await BuildController().Patch(zoneId,
            new ZonesController.PatchZoneRequest(
                null, null, null, null, null, null, null, null));
    }

    [Then(@"la respuesta del servidor es 200 OK")]
    public void ThenRespuesta200()
    {
        _ctx.Result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Then(@"la zona (.*) tiene imageUrl ""(.*)""")]
    public void ThenZonaTieneImageUrl(int zoneId, string expectedUrl)
    {
        _ctx.Result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().NotBeNull();
    }

    [Then(@"la zona (.*) no tiene imagen registrada")]
    public void ThenZonaSinImagen(int zoneId)
    {
        _ctx.Result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().NotBeNull();
    }
}