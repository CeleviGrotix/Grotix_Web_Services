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
using TechTalk.SpecFlow;

namespace Profiles.Api.Tests.Acceptance.Steps;

[Binding]
public class US26Steps
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserCommandService> _userCommandService = new();
    private readonly Mock<IUserQueryService> _userQueryService = new();
    private readonly Mock<IUserNotificationCommandService> _notificationCommandService = new();
    private readonly Mock<IUserNotificationQueryService> _notificationQueryService = new();
    private readonly Mock<IStaffQueryService> _staffQueryService = new();

    private IActionResult? _result;
    private UpdateUserPreferencesCommand? _capturedCommand;
    private int _userId;

    private UserProfileController BuildController()
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
            new Claim(ClaimTypes.Role, "user_admin"),
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
            new Claim(JwtClaimTypes.IdentityId, _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth", ClaimTypes.NameIdentifier, ClaimTypes.Role);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    private void SetupUser()
    {
        var user = new User(_userId, UserEmail.Create("test@grotix.pe"), 3, "Test User");
        _userQueryService.Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>())).ReturnsAsync(user);
        _mediator.Setup(m => m.Send(It.IsAny<UpdateUserPreferencesCommand>(), default))
            .Callback<IRequest<User>, CancellationToken>((cmd, _) => _capturedCommand = (UpdateUserPreferencesCommand)cmd)
            .ReturnsAsync(user);
    }

    [Given(@"el usuario autenticado tiene id (.*)")]
    public void GivenUsuarioConId(int userId)
    {
        _userId = userId;
        SetupUser();
    }

    [When(@"el usuario activa push y desactiva email")]
    public async Task WhenActivaPushDesactivaEmail()
    {
        _result = await BuildController().PatchPreferences(_userId,
            new UserProfileController.PatchPreferencesRequest(Push: true, Email: false));
    }

    [When(@"el usuario desactiva push y desactiva email")]
    public async Task WhenDesactivaAmbos()
    {
        _result = await BuildController().PatchPreferences(_userId,
            new UserProfileController.PatchPreferencesRequest(Push: false, Email: false));
    }

    [When(@"el usuario activa push y activa email")]
    public async Task WhenActivaAmbos()
    {
        _result = await BuildController().PatchPreferences(_userId,
            new UserProfileController.PatchPreferencesRequest(Push: true, Email: true));
    }

    [Then(@"la preferencia se guarda correctamente con código 200")]
    public void ThenPreferenciaGuardada()
    {
        _result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Then(@"el comando enviado contiene push true y email true")]
    public void ThenComandoContienePushYEmail()
    {
        _capturedCommand.Should().NotBeNull();
        _capturedCommand!.Push.Should().BeTrue();
        _capturedCommand.Email.Should().BeTrue();
    }
}