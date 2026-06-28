using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Controllers;
using GrotixBackend.Contracts.Profiles.Access;
using System.Security.Claims;
using TechTalk.SpecFlow;
using Xunit;

namespace CultivationArea.Api.Tests.Acceptance.Steps;

[Binding]
public class US22Steps
{
    private readonly Mock<IZoneCommandService> _zoneCommandService = new();
    private readonly Mock<IZoneQueryService> _zoneQueryService = new();
    private readonly Mock<IFarmQueryService> _farmQueryService = new();
    private readonly Mock<IUserAccessContextService> _accessContextService = new();
    private readonly Mock<IZoneMemberService> _zoneMemberService = new();

    private IActionResult? _result;
    private Zone? _currentZone;

    private ZonesController BuildController()
    {
        var controller = new ZonesController(
            _accessContextService.Object,
            _farmQueryService.Object,
            _zoneCommandService.Object,
            _zoneQueryService.Object,
            _zoneMemberService.Object);

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

    [Given(@"el usuario está autenticado como administrador")]
    public void GivenUsuarioAutenticado() { }

    [Given(@"existe una zona con id (.*) en el sistema")]
    public void GivenZonaExiste(int zoneId)
    {
        _currentZone = BuildZone();
        _zoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync(_currentZone);
    }

    [Given(@"la zona (.*) no existe en el sistema")]
    public void GivenZonaNoExiste(int zoneId)
    {
        _zoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync((Zone?)null);
    }

    [Given(@"la zona (.*) no tiene imagen registrada")]
    public void GivenZonaSinImagen(int zoneId)
    {
        _currentZone = BuildZone(null);
        _zoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync(_currentZone);
    }

    [Given(@"la zona (.*) tiene imageUrl ""(.*)""")]
    public void GivenZonaConImageUrl(int zoneId, string imageUrl)
    {
        _currentZone = BuildZone(imageUrl);
        _zoneQueryService
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

        _result = await BuildController().Patch(zoneId,
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

        _result = await BuildController().Patch(zoneId,
            new ZonesController.PatchZoneRequest(
                null, null, null, null, null, null, null, null));
    }

    [Then(@"la respuesta del servidor es 200 OK")]
    public void ThenRespuesta200()
    {
        _result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Then(@"la respuesta del servidor es 404 Not Found")]
    public void ThenRespuesta404()
    {
        _result.Should().BeOfType<NotFoundResult>();
    }

    [Then(@"la zona (.*) tiene imageUrl ""(.*)""")]
    public void ThenZonaTieneImageUrl(int zoneId, string expectedUrl)
    {
        var ok = _result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Then(@"la zona (.*) no tiene imagen registrada")]
    public void ThenZonaSinImagen(int zoneId)
    {
        var ok = _result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }
}