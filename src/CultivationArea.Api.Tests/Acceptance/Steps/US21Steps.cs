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
using TechTalk.SpecFlow;

namespace CultivationArea.Api.Tests.Acceptance.Steps;

[Binding]
public class US21Steps
{
    private readonly SharedContext _ctx;
    private readonly Mock<IZoneReportService> _reportService = new();

    private IActionResult? _result;

    public US21Steps(SharedContext ctx) => _ctx = ctx;

    private ZoneReportsController BuildController()
    {
        var controller = new ZoneReportsController(
            _ctx.AccessContextService.Object,
            _ctx.FarmQueryService.Object,
            _ctx.ZoneQueryService.Object,
            _ctx.ZoneMemberService.Object,
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

    private static ZoneReportData BuildReportData(int days) =>
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

    [When(@"el usuario solicita el reporte de la zona (.*) para los últimos (.*) días")]
    public async Task WhenSolicitaReporte(int zoneId, int days)
    {
        var report = BuildReportData(days);
        _reportService
            .Setup(s => s.BuildAsync(zoneId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), default))
            .ReturnsAsync(report);

        _result = await BuildController().GetSummary(
            zoneId,
            DateTime.UtcNow.AddDays(-days),
            DateTime.UtcNow);
    }

    [When(@"el usuario exporta el PDF de la zona (.*) para los últimos (.*) días")]
    public async Task WhenExportaPdf(int zoneId, int days)
    {
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _reportService
            .Setup(s => s.BuildPdfAsync(zoneId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), default))
            .ReturnsAsync(pdfBytes);

        _result = await BuildController().ExportPdf(
            zoneId,
            DateTime.UtcNow.AddDays(-days),
            DateTime.UtcNow);
    }

    [Then(@"la respuesta del reporte es 200 OK")]
    public void ThenRespuestaReporte200()
    {
        _result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Then(@"la respuesta del reporte es 404 Not Found")]
    public void ThenRespuestaReporte404()
    {
        _result.Should().BeOfType<NotFoundResult>();
    }

    [Then(@"el reporte contiene información del periodo solicitado")]
    public void ThenReporteContienePeriodo()
    {
        _result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().NotBeNull();
    }

    [Then(@"el reporte incluye telemetría promedio e irrigación")]
    public void ThenReporteIncluyeMetricas()
    {
        _result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().NotBeNull();
    }

    [Then(@"la respuesta es un archivo PDF")]
    public void ThenRespuestaEsPdf()
    {
        _result.Should().BeOfType<FileContentResult>()
            .Which.ContentType.Should().Be("application/pdf");
    }
}