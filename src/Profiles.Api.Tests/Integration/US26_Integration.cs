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

/// <summary>
/// US26 — Configuración y gestión de alertas de usuario
/// </summary>
public class US26_NotificationPreferencesIntegrationTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserCommandService> _userCommandService = new();
    private readonly Mock<IUserQueryService> _userQueryService = new();
    private readonly Mock<IUserNotificationCommandService> _notificationCommandService = new();
    private readonly Mock<IUserNotificationQueryService> _notificationQueryService = new();
    private readonly Mock<IStaffQueryService> _staffQueryService = new();

    private void SetupUserWithId(int userId)
    {
        var user = new User(userId, UserEmail.Create("test@grotix.pe"), 3, "Test User");
        _userQueryService
            .Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>()))
            .ReturnsAsync(user);
        // Bypass the caller.Id != userId check by making controller think caller IS the user
        // We do this by mocking as admin
    }

    private UserProfileController BuildController(int userId = 1)
    {
        var controller = new UserProfileController(
            _mediator.Object,
            _userCommandService.Object,
            _userQueryService.Object,
            _notificationCommandService.Object,
            _notificationQueryService.Object,
            _staffQueryService.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "admin"),  // ← admin bypasses the caller.Id check
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimTypes.IdentityId, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth", ClaimTypes.NameIdentifier, ClaimTypes.Role);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    private static User BuildUser(int id = 1) =>
        new User(id, UserEmail.Create("test@grotix.pe"), 3, "Test User");

    // Escenario 1 — Activar/Desactivar notificaciones

    [Fact]
    public async Task PatchPreferences_EnablePush_Returns200()
    {
        var user = BuildUser();
        _userQueryService.Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);
        _mediator.Setup(m => m.Send(It.IsAny<UpdateUserPreferencesCommand>(), default)).ReturnsAsync(user);

        var result = await BuildController().PatchPreferences(1,
            new UserProfileController.PatchPreferencesRequest(Push: true, Email: false));

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task PatchPreferences_EnableEmail_Returns200()
    {
        var user = BuildUser();
        _userQueryService.Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);
        _mediator.Setup(m => m.Send(It.IsAny<UpdateUserPreferencesCommand>(), default)).ReturnsAsync(user);

        var result = await BuildController().PatchPreferences(1,
            new UserProfileController.PatchPreferencesRequest(Push: false, Email: true));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PatchPreferences_DisableBothChannels_Returns200()
    {
        var user = BuildUser();
        _userQueryService.Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);
        _mediator.Setup(m => m.Send(It.IsAny<UpdateUserPreferencesCommand>(), default)).ReturnsAsync(user);

        var result = await BuildController().PatchPreferences(1,
            new UserProfileController.PatchPreferencesRequest(Push: false, Email: false));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PatchPreferences_SendsCorrectCommand()
    {
        var user = BuildUser();
        _userQueryService.Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);

        UpdateUserPreferencesCommand? captured = null;
        _mediator.Setup(m => m.Send(It.IsAny<UpdateUserPreferencesCommand>(), default))
            .Callback<IRequest<User>, CancellationToken>((cmd, _) => captured = (UpdateUserPreferencesCommand)cmd)
            .ReturnsAsync(user);

        await BuildController().PatchPreferences(1,
            new UserProfileController.PatchPreferencesRequest(Push: true, Email: true));

        captured.Should().NotBeNull();
        captured!.Push.Should().BeTrue();
        captured.Email.Should().BeTrue();
        captured.UserId.Should().Be(1);
    }

    [Fact]
    public async Task PatchPreferences_UserNotFound_Returns404()
    {
        var user = BuildUser();
        _userQueryService.Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);
        _mediator.Setup(m => m.Send(It.IsAny<UpdateUserPreferencesCommand>(), default))
            .ThrowsAsync(new KeyNotFoundException("Usuario 1 no encontrado."));

        var result = await BuildController().PatchPreferences(1,
            new UserProfileController.PatchPreferencesRequest(Push: true, Email: false));

        result.Should().BeOfType<NotFoundResult>();
    }

    // Escenario 3 — Notificaciones

    [Fact]
    public async Task GetMyNotifications_Returns200WithList()
    {
        var user = BuildUser();
        _userQueryService.Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);
        _notificationQueryService.Setup(s => s.ListByUserAsync(1, false, 50))
            .ReturnsAsync(new List<UserNotification>
            {
                new UserNotification(1, "Alerta Crítica", "Humedad baja", "alert")
            });

        var result = await BuildController().GetMyNotifications();

        result.Should().BeOfType<OkObjectResult>();
    }
}