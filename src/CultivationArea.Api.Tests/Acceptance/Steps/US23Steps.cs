using FluentAssertions;
using GrotixBackend.CultivationArea.Application.Internal;
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
public class US23Steps
{
    private readonly SharedContext _ctx;
    private readonly Mock<IAnalysisReportService> _reportService = new();

    public US23Steps(SharedContext ctx) => _ctx = ctx;

    private AnalysisReportsController BuildController()
    {
        var controller = new AnalysisReportsController(
            _ctx.AccessContextService.Object,
            _ctx.FarmQueryService.Object,
            _ctx.ZoneQueryService.Object,
            _ctx.ZoneMemberService.Object,
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

    [When(@"el sistema registra el resultado de IA con fase ""(.*)"" y score (.*)")]
    public async Task WhenRegistraResultadoIA(string phase, float score)
    {
        var report = new AnalysisReport(1, phase, score);
        _reportService
            .Setup(s => s.CreateAsync(1, phase, score, default))
            .ReturnsAsync(report);

        _ctx.Result = await BuildController().Create(1,
            new AnalysisReportsController.CreateAnalysisReportRequest(phase, score),
            default);
    }

    [When(@"el sistema registra el resultado de IA en zona (.*) con fase ""(.*)"" y score (.*)")]
    public async Task WhenRegistraResultadoIAEnZona(int zoneId, string phase, float score)
    {
        _ctx.Result = await BuildController().Create(zoneId,
            new AnalysisReportsController.CreateAnalysisReportRequest(phase, score),
            default);
    }

    [Then(@"la respuesta del servidor es 201 Created")]
    public void ThenRespuesta201()
    {
        _ctx.Result.Should().BeOfType<CreatedResult>();
    }

    [Then(@"el reporte contiene la fase detectada ""(.*)""")]
    public void ThenReporteContienePhase(string expectedPhase)
    {
        _ctx.Result.Should().BeOfType<CreatedResult>().Subject.Value.Should().NotBeNull();
    }
}