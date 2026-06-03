using FluentAssertions;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Services;
using TechTalk.SpecFlow;

namespace Telemetry.Api.Tests.Acceptance.Steps;

[Binding]
public class TelemetryIngestSteps
{
    private Sensor _sensor = new();
    private double _inputValue;
    private bool _isValid;
    
    private List<double> _history = new();
    private double _smoothedResult;

    private double _minThreshold;
    private double _maxThreshold;
    private bool _isOutOfRange;
    private string _breachDirection = string.Empty;

    // --- US11: Anomalías ---
    [Given(@"un sensor con rango fisico entre (.*) y (.*)")]
    public void GivenUnSensorConRangoFisico(double min, double max)
    {
        _sensor.MinPhysical = min;
        _sensor.MaxPhysical = max;
    }

    [When(@"el sensor envia una lectura de (.*)")]
    public void WhenElSensorEnviaUnaLectura(double value)
    {
        _inputValue = value;
        _isValid = ReadingRangeValidator.IsPhysicallyValid(_sensor, value);
    }

    [Then(@"el sistema debe marcar la lectura como invalida")]
    public void ThenElSistemaDebeMarcarComoInvalida()
    {
        _isValid.Should().BeFalse();
    }

    // --- US13: Media Móvil ---
    [Given(@"que el sensor ha registrado valores previos de (.*) y (.*)")]
    public void GivenValoresPrevios(double val1, double val2)
    {
        _history = new List<double> { val1, val2 };
    }

    [When(@"llega una nueva lectura de (.*) con una ventana de (.*)")]
    public void WhenLlegaNuevaLecturaConVentana(double newValue, int window)
    {
        _smoothedResult = MovingAverageFilter.Smooth(_history, newValue, window);
    }

    [Then(@"el sistema debe calcular un promedio suavizado de (.*)")]
    public void ThenElSistemaDebeCalcularPromedio(double expected)
    {
        _smoothedResult.Should().Be(expected);
    }

    // --- US18: Umbrales ---
    [Given(@"que el umbral minimo es (.*) y el maximo es (.*)")]
    public void GivenUmbrales(double min, double max)
    {
        _minThreshold = min;
        _maxThreshold = max;
    }

    [When(@"se evalua una lectura de (.*)")]
    public void WhenSeEvaluaLectura(double value)
    {
        _inputValue = value;
        _isOutOfRange = ThresholdEvaluator.IsOutOfRange(_inputValue, _minThreshold, _maxThreshold);
        _breachDirection = ThresholdEvaluator.GetBreachDirection(_inputValue, _minThreshold, _maxThreshold);
    }

    [Then(@"el sistema debe determinar que esta fuera de rango")]
    public void ThenElSistemaDebeDeterminarFueraDeRango()
    {
        _isOutOfRange.Should().BeTrue();
    }

    [Then(@"la direccion de la brecha debe ser ""(.*)""")]
    public void ThenLaDireccionDeBrechaDebeSer(string expectedDirection)
    {
        _breachDirection.Should().Be(expectedDirection);
    }
}