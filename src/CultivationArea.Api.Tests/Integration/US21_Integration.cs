using FluentAssertions;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;
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
/// US21 — Generación y descarga de reportes históricos
/// </summary>
public class US21_ReportGenerationIntegrationTests
{
    private readonly Mock<IZoneReportService> _reportService = new();
    private readonly Mock<IZoneQueryService> _zoneQueryService = new();
    private readonly Mock<IFarmQueryService> _farmQueryService = new();
    private readonly Mock<IUserAccessContextService> _accessContextService = new();
    private readonly Mock<IZoneMemberService> _zoneMemberService = new();

    private ZoneReportsController BuildController()
    {
        var controller = new ZoneReportsController(
            _accessContextService.Object,
            _farmQueryService.Object,
            _zoneQueryService.Object,
            _zoneMemberService.Object,
            _reportService.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.NameIdentifier, "100"),
            new Claim(JwtClaimTypes.IdentityId, "100")
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

    private static ZoneReportData BuildReportData(int days = 30) =>
        new ZoneReportData(
            GeneratedAtUtc: DateTime.UtcNow,
            PeriodStartUtc: DateTime.UtcNow.AddDays(-days),
            PeriodEndUtc: DateTime.UtcNow,
            Zone: new ZoneReportZoneInfo(1, "Zona Test", "Tomate", "AUTOMATIC", "Vegetativo", -12.0, -77.0),
            Farm: new ZoneReportFarmInfo(1, "Granja Central", "Lima", 1, "Asociación Test"),
            Devices: new List<ZoneReportDeviceInfo>(),
            Telemetry: new ZoneReportTelemetrySummary(100, 22.5, 65.0, 45.0, 80.0),
            Irrigation: new ZoneReportIrrigationSummary(5, 120.5, 60),
            AnalysisReports: new List<ZoneReportAnalysisInfo>(),
            Alerts: new List<ZoneReportAlertInfo>());

    // Escenario 2 — Vista previa de métricas

    [Fact]
    public async Task GetSummary_WithValidZoneAndRange_Returns200()
    {
        var zone = BuildZone();
        var report = BuildReportData(30);
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.BuildAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), default))
            .ReturnsAsync(report);

        var result = await BuildController().GetSummary(1, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(365)]
    public async Task GetSummary_AcceptsAllPredefinedRanges(int days)
    {
        var zone = BuildZone();
        var report = BuildReportData(days);
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.BuildAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), default))
            .ReturnsAsync(report);

        var result = await BuildController().GetSummary(
            1, DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSummary_WithNonExistentZone_Returns404()
    {
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>()))
            .ReturnsAsync((Zone?)null);

        var result = await BuildController().GetSummary(99999, null, null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSummary_ReturnsNullReport_Returns404()
    {
        var zone = BuildZone();
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.BuildAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), default))
            .ReturnsAsync((ZoneReportData?)null);

        var result = await BuildController().GetSummary(1, null, null);

        result.Should().BeOfType<NotFoundResult>();
    }

    // Escenario 3 — Exportación PDF

    [Fact]
    public async Task ExportPdf_WithValidZoneAndRange_ReturnsFileResult()
    {
        var zone = BuildZone();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.BuildPdfAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), default))
            .ReturnsAsync(pdfBytes);

        var result = await BuildController().ExportPdf(1, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ExportPdf_ReturnsPdfContentType()
    {
        var zone = BuildZone();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>())).ReturnsAsync(zone);
        _reportService.Setup(s => s.BuildPdfAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), default))
            .ReturnsAsync(pdfBytes);

        var result = await BuildController().ExportPdf(1, null, null);

        result.Should().BeOfType<FileContentResult>()
            .Which.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task ExportPdf_WithNonExistentZone_Returns404()
    {
        _zoneQueryService.Setup(s => s.Handle(It.IsAny<GetZoneByIdQuery>()))
            .ReturnsAsync((Zone?)null);

        var result = await BuildController().ExportPdf(99999, null, null);

        result.Should().BeOfType<NotFoundResult>();
    }
}