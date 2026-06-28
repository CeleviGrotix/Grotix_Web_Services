using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CultivationArea.Api.Tests.Acceptance.Steps;

/// <summary>Contexto compartido entre steps de distintos archivos en el mismo escenario.</summary>
public class SharedContext
{
    public Mock<IZoneQueryService> ZoneQueryService { get; } = new();
    public Mock<IFarmQueryService> FarmQueryService { get; } = new();
    public Mock<IUserAccessContextService> AccessContextService { get; } = new();
    public Mock<IZoneMemberService> ZoneMemberService { get; } = new();
    public IActionResult? Result { get; set; }
}