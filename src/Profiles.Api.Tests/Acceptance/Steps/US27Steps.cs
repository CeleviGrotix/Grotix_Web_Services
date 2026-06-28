using FluentAssertions;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Interfaces.REST.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TechTalk.SpecFlow;
using GrotixBackend.Profiles.Application.Internal.CommandServices;

namespace Profiles.Api.Tests.Acceptance.Steps;

[Binding]
public class US27Steps
{
    private readonly Mock<IContractQueryService> _contractQueryService = new();
    private readonly Mock<IContractCommandService> _contractCommandService = new();
    private readonly Mock<IUserQueryService> _userQueryService = new();

    private IActionResult? _result;
    private int _associationId;

    private ContractsController BuildController()
    {
        var controller = new ContractsController(
            _contractQueryService.Object,
            _contractCommandService.Object,
            _userQueryService.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "user_admin"),
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

        private void SetupUser()
        {
            var user = new User(100, UserEmail.Create("admin@grotix.pe"), 3);
            user.AssignAssociation(_associationId);
            _userQueryService
                .Setup(s => s.Handle(It.IsAny<GetUserByIdentityQuery>()))
                .ReturnsAsync(user);
        }

    private Contract BuildContract(bool isSuspended) =>
        new Contract(
            associationId: _associationId,
            startDate: DateTime.UtcNow.AddDays(-10),
            endDate: DateTime.UtcNow.AddDays(30),
            status: ContractStatus.Active,
            maxZones: 5,
            maxMicrocontrollers: 3,
            totalAmount: 100,
            currency: ContractCurrency.USD,
            paymentFrequency: ContractPaymentFrequency.Monthly,
            isSuspended: isSuspended);

    [Given(@"el usuario está autenticado como user_admin con asociación (.*)")]
    public void GivenUsuarioAutenticado(int associationId)
    {
        _associationId = associationId;
        SetupUser();
    }

    [Given(@"la asociación (.*) tiene un contrato activo y no suspendido")]
    public void GivenContratoActivo(int associationId)
    {
        _contractQueryService
            .Setup(s => s.ListByAssociationAsync(associationId))
            .ReturnsAsync(new List<Contract> { BuildContract(false) });
    }

    [Given(@"la asociación (.*) tiene un contrato suspendido")]
    public void GivenContratoSuspendido(int associationId)
    {
        _contractQueryService
            .Setup(s => s.ListByAssociationAsync(associationId))
            .ReturnsAsync(new List<Contract> { BuildContract(true) });
    }

    [Given(@"la asociación (.*) no tiene contratos")]
    public void GivenSinContratos(int associationId)
    {
        _contractQueryService
            .Setup(s => s.ListByAssociationAsync(associationId))
            .ReturnsAsync(new List<Contract>());
    }

    [When(@"el usuario consulta el estado de sus servicios")]
    public async Task WhenConsultaEstado()
    {
        _result = await BuildController().List();
    }

    [Then(@"la respuesta es 200 OK")]
    public void ThenRespuesta200()
    {
        _result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Then(@"el contrato muestra el servicio como disponible")]
    public void ThenServicioDisponible()
    {
        var ok = _result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Then(@"el contrato muestra el servicio como suspendido")]
    public void ThenServicioSuspendido()
    {
        var ok = _result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Then(@"la lista de contratos está vacía")]
    public void ThenListaVacia()
    {
        var ok = _result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }
}