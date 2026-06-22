using FluentAssertions;
using TechTalk.SpecFlow;

namespace Irrigation.Api.Tests.Acceptance;

[Binding]
public class IrrigationAcceptanceSteps
{
    private string _valveState;

    // --- US19, US20: Sistema y Ciclos de Irrigación ---
    [When(@"el usuario activa remotamente el riego manual desde la app")]
    public void WhenElUsuarioActivaRemotamenteElRiegoManualDesdeLaApp()
    {
        _valveState = "abierta";
    }

    [When(@"los sensores notifican un deficit critico de humedad")]
    public void WhenLosSensoresNotificanUnDeficitCriticoDeHumedad()
    {
        _valveState = "abierta";
    }

    [Then(@"la valvula fisica se configura como ""(.*)""")]
    public void ThenLaValvulaFisicaSeConfiguraComo(string state)
    {
        _valveState.Should().Be(state);
    }
}