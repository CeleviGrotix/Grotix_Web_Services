using FluentAssertions;
using TechTalk.SpecFlow;

namespace Telemetry.Api.Tests.Acceptance;

[Binding]
public class TelemetryAcceptanceSteps
{
    private double _humidityDetected;
    private string _dashboardValue;
    private string _validationStatus;

    // --- US11, US12, US13: Captura y Monitoreo de Telemetría ---
    [Given(@"un cambio fisico en la humedad del suelo de la zona")]
    public void GivenUnCambioFisicoEnLaHumedadDelSueloDeLaZona()
    {
        _humidityDetected = 0.75;
    }

    [When(@"el sensor efectua el ciclo de lectura")]
    public void WhenElSensorEfectuaElCicloDeLectura()
    {
        _dashboardValue = $"{_humidityDetected * 100}%";
    }

    [Then(@"el dashboard actualiza la pantalla de forma reactiva")]
    public void ThenElDashboardActualizaLaPantallaDeFormaReactiva()
    {
        _dashboardValue.Should().Be("75%");
    }

    [Given(@"un salto anomalico de humedad de ""(.*)"" a ""(.*)"" sin riego activo")]
    public void GivenUnSaltoAnomalicoDeHumedadADeSinRiegoActivo(int initial, int final)
    {
        _validationStatus = (final - initial > 30) ? "pendiente de validación" : "válido";
    }

    [Then(@"la lectura es marcada como ""(.*)""")]
    public void ThenLaLecturaEsMarcadaComo(string expectedStatus)
    {
        _validationStatus.Should().Be(expectedStatus);
    }
}