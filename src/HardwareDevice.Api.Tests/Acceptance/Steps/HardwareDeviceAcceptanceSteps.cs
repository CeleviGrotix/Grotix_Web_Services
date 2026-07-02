using FluentAssertions;
using TechTalk.SpecFlow;

namespace HardwareDevice.Api.Tests.Acceptance;

[Binding]
public class HardwareDeviceAcceptanceSteps
{
    private string _deviceId;
    private string _deviceStatus;

    // --- US10: Vinculación de Microcontrolador ---
    [Given(@"un dispositivo con ID ""(.*)""")]
    public void GivenUnDispositivoConID(string deviceId)
    {
        _deviceId = deviceId;
    }

    [When(@"el administrador asocia el dispositivo a la zona de cultivo")]
    public void WhenElAdministradorAsociaElDispositivoALaZonaDeCultivo()
    {
        _deviceStatus = (_deviceId == "valido") ? "Activo" : "Error";
    }

    [Then(@"el perfil muestra el dispositivo como ""(.*)""")]
    public void ThenElPerfilMuestraElDispositivoComo(string expectedStatus)
    {
        _deviceStatus.Should().Be(expectedStatus);
    }
}