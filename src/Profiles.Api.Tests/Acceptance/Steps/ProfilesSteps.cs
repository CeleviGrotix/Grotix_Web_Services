using FluentAssertions;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using TechTalk.SpecFlow;

namespace Profiles.Api.Tests.Acceptance.Steps;

[Binding]
public class ProfilesSteps
{
    private User _user = default!;
    private Contract _contract = default!;

    // --- US26: Modificar Perfil ---
    [Given(@"un usuario registrado con el nombre ""(.*)""")]
    public void GivenUnUsuarioRegistrado(string name)
    {
        _user = new User(100, UserEmail.Create("a@a.com"), 4, name);
    }

    [When(@"el usuario actualiza su nombre a ""(.*)"" y su telefono a ""(.*)""")]
    public void WhenActualizaNombreYTelefono(string newName, string phone)
    {
        _user.UpdateProfile(newName, null, phone, null);
    }

    [Then(@"los datos del perfil deben reflejar el nombre ""(.*)""")]
    public void ThenLosDatosDebenReflejarElNombre(string expectedName)
    {
        _user.Name.Should().Be(expectedName);
    }

    // --- Contratos: Actualización de Límites ---
    [Given(@"un contrato comercial activo con limite de (.*) zonas")]
    public void GivenUnContratoActivo(int zones)
    {
        _contract = new Contract(1, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ContractStatus.Active, zones, 5, 100, ContractCurrency.USD, ContractPaymentFrequency.Monthly, false);
    }

    [When(@"el administrador actualiza el limite maximo a (.*) zonas")]
    public void WhenAdministradorActualizaLimite(int newZones)
    {
        _contract.Update(null, null, newZones, null, null, null, null, null);
    }

    [Then(@"el contrato debe permitir hasta (.*) zonas operativas")]
    public void ThenContratoPermiteZonas(int expectedZones)
    {
        _contract.MaxZones.Should().Be(expectedZones);
    }

    // --- Contratos: Suspensión (US11) ---
    [Given(@"un contrato comercial en estado ""Active"" no suspendido")]
    public void GivenUnContratoNoSuspendido()
    {
        _contract = new Contract(1, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ContractStatus.Active, 10, 5, 100, ContractCurrency.USD, ContractPaymentFrequency.Monthly, false);
    }

    [When(@"el administrador ejecuta la suspension del contrato")]
    public void WhenAdministradorSuspendeContrato()
    {
        _contract.Update(null, null, null, null, true, null, null, null);
    }

    [Then(@"la bandera de suspension del contrato debe estar activa")]
    public void ThenBanderaDeSuspensionEstaActiva()
    {
        _contract.IsSuspended.Should().BeTrue();
    }
}
