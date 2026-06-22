using FluentAssertions;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Interfaces.REST.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Profiles.Api.Tests.Integration;

public class UserProfileControllerTests
{
    private UserProfileController SetupController(int tokenIdentityId, string role, Mock<IUserQueryService> queryService, Mock<IUserCommandService> commandService)
    {
        var controller = new UserProfileController(
            new Mock<IMediator>().Object,
            commandService.Object,
            queryService.Object,
            new Mock<IUserNotificationCommandService>().Object,
            new Mock<IUserNotificationQueryService>().Object,
            new Mock<IStaffQueryService>().Object);

        var claims = new[] { 
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtClaimTypes.IdentityId, tokenIdentityId.ToString()) 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth", ClaimTypes.NameIdentifier, ClaimTypes.Role);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    [Fact]
    public async Task GetMe_ValidToken_ReturnsOkWithProfile()
    {
        // Arrange
        var mockQuery = new Mock<IUserQueryService>();
        var email = UserEmail.Create("test@grotix.pe");
        // Simulamos que al buscar la identidad 100, retorna el usuario ID 1
        mockQuery.Setup(q => q.Handle(It.Is<GetUserByIdentityQuery>(qry => qry.IdentityId == 100)))
                 .ReturnsAsync(new User(100, UserEmail.Create("test@test.com"), 4, "Juan Perez"));

        var controller = SetupController(100, "user", mockQuery, new Mock<IUserCommandService>());

        // Act
        var result = await controller.GetMe();

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task PatchProfile_SameUser_ReturnsOk()
    {
        // Arrange
        var mockQuery = new Mock<IUserQueryService>();
        var user = new User(100, UserEmail.Create("test@test.com"), 4, "Viejo Nombre");
        // Usamos reflection para setear el ID privado para propósitos del test
        typeof(User).GetProperty("Id")?.SetValue(user, 1);

        mockQuery.Setup(q => q.Handle(It.Is<GetUserByIdentityQuery>(qry => qry.IdentityId == 100)))
                 .ReturnsAsync(user);

        var mockCommand = new Mock<IUserCommandService>();
        mockCommand.Setup(c => c.Handle(It.IsAny<UpdateUserProfileCommand>()))
                   .ReturnsAsync(user);

        var controller = SetupController(100, "user", mockQuery, mockCommand);
        var request = new UserProfileController.PatchProfileRequest("Nuevo", null, null, null);

        // Act
        var result = await controller.PatchProfile(1, request); // Parcheando su propio ID (1)

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task PatchProfile_DifferentUserNotAdmin_ReturnsForbid()
    {
        // Arrange
        var mockQuery = new Mock<IUserQueryService>();
        var user = new User(100, UserEmail.Create("test@test.com"), 4, "Nombre");
        typeof(User).GetProperty("Id")?.SetValue(user, 1);

        // El usuario logueado es el ID 1, pero intenta modificar el perfil ID 2
        mockQuery.Setup(q => q.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);

        var controller = SetupController(100, "user_basic", mockQuery, new Mock<IUserCommandService>());
        var request = new UserProfileController.PatchProfileRequest("Hack", null, null, null);

        // Act
        var result = await controller.PatchProfile(2, request); 

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }
}
