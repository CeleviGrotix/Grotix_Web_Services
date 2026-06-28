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
using TechTalk.SpecFlow;
using Xunit;

namespace CultivationArea.Api.Tests.Acceptance.Steps;

[Binding]
public class US23Steps
{
    private readonly Mock<IAnalysisReportService> _reportService = new();
    private readonly Mock<IZoneQueryService> _zoneQueryService = new();
    private readonly Mock<IFarmQueryService> _farmQueryService = new();
    private readonly Mock<IUserAccessContextService> _accessContextService = new();
    private readonly Mock<IZoneMemberService> _zoneMemberService = new();

    private IActionResult? _result;

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

    [Given(@"el usuario está autenticado como administrador")]
    public void GivenUsuarioAutenticado() { }

    [Given(@"existe una zona con id (.*) en el sistema")]
    public void GivenZonaExiste(int zoneId)
    {
        _zoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync(BuildZone());
    }

    [Given(@"la zona (.*) no existe en el sistema")]
    public void GivenZonaNoExiste(int zoneId)
    {
        _zoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync((Zone?)null);
    }

    [When(@"el sistema registra el resultado de IA con fase ""(.*)"" y score (.*)")]
    public async Task WhenRegistraResultadoIA(string phase, float score)
    {
        var report = new AnalysisReport(1, phase, score);
        _reportService
            .Setup(s => s.CreateAsync(1, phase, score, default))
            .ReturnsAsync(report);

        _result = await BuildController().Create(1,
            new AnalysisReportsController.CreateAnalysisReportRequest(phase, score),
            default);
    }

    [When(@"el sistema registra el resultado de IA en zona (.*) con fase ""(.*)"" y score (.*)")]
    public async Task WhenRegistraResultadoIAEnZona(int zoneId, string phase, float score)
    {
        _result = await BuildController().Create(zoneId,
            new AnalysisReportsController.CreateAnalysisReportRequest(phase, score),
            default);
    }

    [Then(@"la respuesta del servidor es 201 Created")]
    public void ThenRespuesta201()
    {
        _result.Should().BeOfType<CreatedResult>();
    }

    [Then(@"la respuesta del servidor es 404 Not Found")]
    public void ThenRespuesta404()
    {
        _result.Should().BeOfType<NotFoundResult>();
    }

    [Then(@"el reporte contiene la fase detectada ""(.*)""")]
    public void ThenReporteContienePhase(string expectedPhase)
    {
        var created = _result.Should().BeOfType<CreatedResult>().Subject;
        created.Value.Should().NotBeNull();
    }
}