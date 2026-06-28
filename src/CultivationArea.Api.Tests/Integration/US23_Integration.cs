using FluentAssertions;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CultivationArea.Api.Tests.Integration;

/// <summary>
/// US23 — Clasificación del estado fenológico mediante IA
/// Integration tests: validan el endpoint POST /api/v1/zones/{id}/analysis-reports
/// </summary>
public class US23_AiClassificationIntegrationTests
{
    private readonly Mock<IAnalysisReportService> _reportService = new();
    private readonly Mock<IZoneQueryService> _zoneQueryService = new();
    private readonly Mock<IFarmQueryService> _farmQueryService = new();
    private readonly Mock<IUserAccessContextService> _accessContextService = new();
    private readonly Mock<IZoneMemberService> _zoneMemberService = new();

    private AnalysisReportsController BuildController()
    {
        var controller = new AnalysisReportsController(
            _accessContextService.Object,
            _farmQueryService.Object,
            _zoneQueryService.Object,
            _zoneMemberService.Object,
            _reportService.Object);

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

    private static Zone BuildZone() =>
        new Zone(farmId: 1, cropId: 1, name: "Zona Test", latitude: -12.0, longitude: -77.0);

    // Escenario 1 — Categorización exitosa

    [Fact]
    public async Task Create_WithValidPhaseAndScore_Returns201()
    {
        var zone = BuildZone();
        var report = new AnalysisReport(1, "Germinacion", 85f);

        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.CreateAsync(1, "Germinacion", 85f, default)).ReturnsAsync(report);

        var result = await BuildController().Create(1,
            new AnalysisReportsController.CreateAnalysisReportRequest("Germinacion", 85f),
            default);

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Create_ReturnsReportWithDetectedPhase()
    {
        var zone = BuildZone();
        var report = new AnalysisReport(1, "Germinacion", 85f);

        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.CreateAsync(1, "Germinacion", 85f, default)).ReturnsAsync(report);

        var result = await BuildController().Create(1,
            new AnalysisReportsController.CreateAnalysisReportRequest("Germinacion", 85f),
            default);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Value.Should().NotBeNull();
    }

    // Escenario 2 — Umbral de confianza

    [Fact]
    public async Task Create_WithIndeterminadoPhase_Returns201()
    {
        var zone = BuildZone();
        var report = new AnalysisReport(1, "Indeterminado", 50f);

        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.CreateAsync(1, "Indeterminado", 50f, default)).ReturnsAsync(report);

        var result = await BuildController().Create(1,
            new AnalysisReportsController.CreateAnalysisReportRequest("Indeterminado", 50f),
            default);

        result.Should().BeOfType<CreatedResult>();
    }

    // Escenario 3 — Zona inexistente

    [Fact]
    public async Task Create_WithNonExistentZone_Returns404()
    {
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>()))
            .ReturnsAsync((Zone?)null);

        var result = await BuildController().Create(99999,
            new AnalysisReportsController.CreateAnalysisReportRequest("Germinacion", 85f),
            default);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_PassesCorrectDataToService()
    {
        var zone = BuildZone();
        var report = new AnalysisReport(1, "Floracion", 90f);

        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);

        string? capturedPhase = null;
        float capturedScore = 0;
        _reportService.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<float>(), default))
            .Callback<int, string, float, CancellationToken>((_, p, s, _) => { capturedPhase = p; capturedScore = s; })
            .ReturnsAsync(report);

        await BuildController().Create(1,
            new AnalysisReportsController.CreateAnalysisReportRequest("Floracion", 90f),
            default);

        capturedPhase.Should().Be("Floracion");
        capturedScore.Should().Be(90f);
    }
}