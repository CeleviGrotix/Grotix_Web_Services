using FluentAssertions;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Interfaces.REST.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;
using GrotixBackend.Profiles.Application.Internal.CommandServices;

namespace Profiles.Api.Tests.Integration;

/// <summary>
/// US27 — Visualización del estado de servicios
/// Integration tests: validan GET /api/v1/contracts
/// </summary>
public class US27_ContractStatusIntegrationTests
{
    private readonly Mock<IContractQueryService> _contractQueryService = new();
    private readonly Mock<IContractCommandService> _contractCommandService = new();
    private readonly Mock<IUserQueryService> _userQueryService = new();

    private ContractsController BuildController(string role = "user_admin", int associationId = 1)
    {
        var controller = new ContractsController(
            _contractQueryService.Object,
            _contractCommandService.Object,
            _userQueryService.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.Role, role),
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

    private static Contract BuildContract(bool isSuspended = false) =>
        new Contract(
            associationId: 1,
            startDate: DateTime.UtcNow.AddDays(-10),
            endDate: DateTime.UtcNow.AddDays(30),
            status: ContractStatus.Active,
            maxZones: 5,
            maxMicrocontrollers: 3,
            totalAmount: 100,
            currency: ContractCurrency.USD,
            paymentFrequency: ContractPaymentFrequency.Monthly,
            isSuspended: isSuspended);

        private void SetupUser(int associationId = 1)
        {
            var user = new GrotixBackend.Profiles.Domain.Model.Aggregates.User(
                100,
                GrotixBackend.Profiles.Domain.Model.ValueObjects.UserEmail.Create("admin@grotix.pe"),
                3);
            user.AssignAssociation(associationId);
            _userQueryService
                .Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>()))
                .ReturnsAsync(user);
        }

    // Escenario 1 — Consulta de estado de servicios

    [Fact]
    public async Task List_AsUserAdmin_Returns200WithContracts()
    {
        SetupUser();
        _contractQueryService
            .Setup(s => s.ListByAssociationAsync(1))
            .ReturnsAsync(new List<Contract> { BuildContract() });

        var result = await BuildController().List();

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ReturnsActiveContract_WhenServiceIsEnabled()
    {
        SetupUser();
        var contract = BuildContract(isSuspended: false);
        _contractQueryService
            .Setup(s => s.ListByAssociationAsync(1))
            .ReturnsAsync(new List<Contract> { contract });

        var result = await BuildController().List();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    // Escenario 2 — Cronograma

    [Fact]
    public async Task List_ReturnsSuspendedContract_WhenServiceIsSuspended()
    {
        SetupUser();
        var contract = BuildContract(isSuspended: true);
        _contractQueryService
            .Setup(s => s.ListByAssociationAsync(1))
            .ReturnsAsync(new List<Contract> { contract });

        var result = await BuildController().List();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    // Escenario 3 — Sin contrato

    [Fact]
    public async Task List_ReturnsEmptyList_WhenNoContractsExist()
    {
        SetupUser();
        _contractQueryService
            .Setup(s => s.ListByAssociationAsync(1))
            .ReturnsAsync(new List<Contract>());

        var result = await BuildController().List();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task List_AsAdmin_Returns200WithAllContracts()
    {
        SetupUser();
        _contractQueryService
            .Setup(s => s.ListAllAsync())
            .ReturnsAsync(new List<Contract> { BuildContract() });

        var result = await BuildController(role: "admin").List();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task List_WithoutAuth_ReturnsUnauthorized()
    {
        var controller = new ContractsController(
            _contractQueryService.Object,
            _contractCommandService.Object,
            _userQueryService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await controller.List();

        result.Should().BeOfType<UnauthorizedResult>();
    }
}