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
using Xunit;

namespace CultivationArea.Api.Tests.Integration;

public class US22_ZoneImageUrlIntegrationTests
{
    private readonly Mock<IZoneCommandService> _zoneCommandService = new();
    private readonly Mock<IZoneQueryService> _zoneQueryService = new();
    private readonly Mock<IFarmQueryService> _farmQueryService = new();
    private readonly Mock<IUserAccessContextService> _accessContextService = new();
    private readonly Mock<IZoneMemberService> _zoneMemberService = new();

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

    [Fact]
    public async Task Patch_WithValidImageUrl_Returns200()
    {
        var zone = BuildZone();
        var updated = BuildZone("https://storage.azure.com/grotix/cultivo.jpg");
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _zoneCommandService.Setup(s => s.Handle(It.IsAny<UpdateZoneCommand>())).ReturnsAsync(updated);

        var result = await BuildController().Patch(1, new ZonesController.PatchZoneRequest(
            null, null, null, null, null, null,
            "https://storage.azure.com/grotix/cultivo.jpg", null));

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Patch_WithNonExistentZone_Returns404()
    {
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>()))
            .ReturnsAsync((Zone?)null);

        var result = await BuildController().Patch(99999, new ZonesController.PatchZoneRequest(
            null, null, null, null, null, null,
            "https://storage.azure.com/grotix/cultivo.jpg", null));

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Patch_WithNullImageUrl_SendsNullToCommand()
    {
        var zone = BuildZone("https://storage.azure.com/grotix/old.jpg");
        var updated = BuildZone(null);
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);

        UpdateZoneCommand? captured = null;
        _zoneCommandService.Setup(s => s.Handle(It.IsAny<UpdateZoneCommand>()))
            .Callback<UpdateZoneCommand>(cmd => captured = cmd)
            .ReturnsAsync(updated);

        await BuildController().Patch(1, new ZonesController.PatchZoneRequest(
            null, null, null, null, null, null, null, null));

        captured!.ImageUrl.Should().BeNull();
    }

    [Fact]
    public async Task Patch_PassesCorrectZoneIdAndImageUrl_ToCommand()
    {
        const string url = "https://storage.azure.com/grotix/cultivo.jpg";
        var zone = BuildZone();
        var updated = BuildZone(url);
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);

        UpdateZoneCommand? captured = null;
        _zoneCommandService.Setup(s => s.Handle(It.IsAny<UpdateZoneCommand>()))
            .Callback<UpdateZoneCommand>(cmd => captured = cmd)
            .ReturnsAsync(updated);

        await BuildController().Patch(1, new ZonesController.PatchZoneRequest(
            null, null, null, null, null, null, url, null));

        captured!.ZoneId.Should().Be(1);
        captured.ImageUrl.Should().Be(url);
    }
}