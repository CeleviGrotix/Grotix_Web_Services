using FluentAssertions;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.CultivationArea.Interfaces.REST.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;
using GrotixBackend.Contracts.Auth.Claims;

namespace CultivationArea.Api.Tests.Integration;

public class CultivationControllerTests
{
    private FarmsController SetupFarmsController(bool isAuthorized, Mock<IFarmCommandService> commandService, string role = "admin")
    {
        var mockAccess = new Mock<IUserAccessContextService>();

        if (isAuthorized)
        {
            mockAccess.Setup(a => a.GetByIdentityIdAsync(100, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UserAccessContext(1, 100, 10, true));
        }

        var controller = new FarmsController(
            mockAccess.Object,
            commandService.Object,
            new Mock<IFarmQueryService>().Object,
            new Mock<IAssociationFarmOwnerSyncService>().Object,
            new Mock<IZoneCommandService>().Object,
            new Mock<IZoneQueryService>().Object,
            new Mock<IZoneMemberRepository>().Object);

        // 👇 Este es el claim que realmente importa
        var claims = new[] {
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtClaimTypes.IdentityId, "100") // <-- el correcto
        };

        var identity = new ClaimsIdentity(claims, "TestAuth", ClaimTypes.NameIdentifier, ClaimTypes.Role);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    [Fact]
    public async Task CreateFarm_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var mockCommand = new Mock<IFarmCommandService>();
        mockCommand.Setup(s => s.Handle(It.IsAny<CreateFarmCommand>()))
                   .ReturnsAsync(new Farm(1, 10, "Granja Central", "Valle Norte"));

        var controller = SetupFarmsController(isAuthorized: true, mockCommand, role: "admin");
        var request = new FarmsController.CreateFarmRequest("Granja Central", "Valle Norte", 10);

        // Act
        var result = await controller.Create(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateFarm_UnauthorizedUserContext_ReturnsUnauthorized()
    {
        // Arrange
        var mockCommand = new Mock<IFarmCommandService>();
        var controller = SetupFarmsController(isAuthorized: false, mockCommand); 
        var request = new FarmsController.CreateFarmRequest("Granja", "Lima", 10);

        // Act
        var result = await controller.Create(request);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task PatchZone_ValidRequest_ReturnsOk()
    {
        // Arrange
        var mockZoneQuery = new Mock<IZoneQueryService>();
        mockZoneQuery.Setup(q => q.Handle(It.IsAny<GrotixBackend.CultivationArea.Domain.Model.Queries.GetZoneByIdQuery>()))
                     .ReturnsAsync(new Zone(1, 2, 10.0, -10.0));

        var mockZoneCommand = new Mock<IZoneCommandService>();
        mockZoneCommand.Setup(c => c.Handle(It.IsAny<UpdateZoneCommand>()))
                       .ReturnsAsync(new Zone(1, 2, 20.0, -20.0));

        var mockAccess = new Mock<IUserAccessContextService>();
        mockAccess.Setup(a => a.GetByIdentityIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new UserAccessContext(1, 1, 10, true));
        mockAccess.Setup(a => a.GetByIdentityIdAsync(It.IsAny<int>()))
                  .ReturnsAsync(new UserAccessContext(1, 1, 10, true));

        var controller = new ZonesController(
            mockAccess.Object,
            new Mock<IFarmQueryService>().Object,
            mockZoneCommand.Object,
            mockZoneQuery.Object,
            new Mock<IZoneMemberService>().Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "admin"), new Claim(ClaimTypes.NameIdentifier, "100") }, "TestAuth", ClaimTypes.NameIdentifier, ClaimTypes.Role)) }
        };

        var request = new ZonesController.PatchZoneRequest(null, 20.0, -20.0, null, null, null);

        // Act
        var result = await controller.Patch(1, request);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }
}