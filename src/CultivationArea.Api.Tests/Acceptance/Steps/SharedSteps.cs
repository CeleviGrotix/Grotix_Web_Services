using FluentAssertions;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TechTalk.SpecFlow;

namespace CultivationArea.Api.Tests.Acceptance.Steps;

/// <summary>Steps compartidos entre todas las historias de usuario.</summary>
[Binding]
public class SharedSteps
{
    private readonly SharedContext _ctx;

    public SharedSteps(SharedContext ctx) => _ctx = ctx;

    [Given(@"el usuario está autenticado como administrador")]
    public void GivenUsuarioAutenticado() { }

    [Given(@"existe una zona con id (.*) en el sistema")]
    public void GivenZonaExiste(int zoneId)
    {
        _ctx.ZoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync(new Zone(farmId: 1, cropId: 1, name: "Zona Test",
                latitude: -12.0, longitude: -77.0));
    }

    [Given(@"la zona (.*) no existe en el sistema")]
    public void GivenZonaNoExiste(int zoneId)
    {
        _ctx.ZoneQueryService
            .Setup(s => s.Handle(It.Is<GetZoneByIdQuery>(q => q.ZoneId == zoneId)))
            .ReturnsAsync((Zone?)null);
    }

    [Then(@"la respuesta del servidor es 404 Not Found")]
    public void ThenRespuesta404()
    {
        _ctx.Result.Should().BeOfType<NotFoundResult>();
    }
}