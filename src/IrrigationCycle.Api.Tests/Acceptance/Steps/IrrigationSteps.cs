using FluentAssertions;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Model.ValueObjects;
using GrotixBackend.IrrigationCycle.Domain.Services;
using TechTalk.SpecFlow;

namespace IrrigationCycle.Api.Tests.Acceptance.Steps;

[Binding]
public class IrrigationSteps
{
    private double _targetHumidity;
    private double _currentHumidity;
    private double _calculatedVolume;
    private int _estimatedDuration;

    private IrrigationCycleRecord _cycle = default!;

    // --- US20: Optimización Hídrica Automática ---
    [Given(@"que el cultivo requiere una humedad objetivo de (.*)%")]
    public void GivenQueElCultivoRequiereHumedadObjetivo(double target)
    {
        _targetHumidity = target;
    }

    [When(@"la humedad actual desciende a (.*)%")]
    public void WhenLaHumedadActualDesciendeA(double current)
    {
        _currentHumidity = current;
        _calculatedVolume = IrrigationCalculator.CalculateVolumeLiters(_currentHumidity, _targetHumidity);
        _estimatedDuration = IrrigationCalculator.EstimateDurationMinutes(_calculatedVolume);
    }

    [Then(@"el sistema debe calcular una necesidad de (.*) litros")]
    public void ThenElSistemaDebeCalcularNecesidad(double expectedVolume)
    {
        _calculatedVolume.Should().Be(expectedVolume);
    }

    [Then(@"el tiempo estimado de riego debe ser (.*) minutos")]
    public void ThenElTiempoEstimadoDeRiegoDebeSer(int expectedMinutes)
    {
        _estimatedDuration.Should().Be(expectedMinutes);
    }

    // --- US19: Activación Manual ---
    [Given(@"un usuario que solicita un riego manual para la zona (.*)")]
    public void GivenUsuarioSolicitaRiegoManual(int zoneId)
    {
        // Se simula la intención de crear un ciclo
    }

    [When(@"se define un volumen de (.*) litros y (.*) minutos de duracion")]
    public void WhenSeDefineVolumenYDuracion(double volume, int duration)
    {
        _cycle = new IrrigationCycleRecord(1, volume, duration);
    }

    [Then(@"el estado del ciclo debe inicializarse como ""(.*)""")]
    public void ThenElEstadoDelCicloDebeInicializarse(string expectedStatus)
    {
        _cycle.Status.Should().Be(expectedStatus);
    }

    // --- US19: Detención Manual ---
    [Given(@"un ciclo de riego activo para la zona (.*)")]
    public void GivenUnCicloDeRiegoActivoParaZona(int zoneId)
    {
        _cycle = new IrrigationCycleRecord(zoneId, 50.0, 10);
    }

    [When(@"el usuario detiene el riego con el motivo ""(.*)""")]
    public void WhenUsuarioDetieneRiego(string reason)
    {
        _cycle.Abort(reason);
    }

    [Then(@"el estado del ciclo debe cambiar a ""(.*)""")]
    public void ThenElEstadoDebeCambiarA(string expectedStatus)
    {
        _cycle.Status.Should().Be(expectedStatus);
    }

    [Then(@"la razon de cancelacion debe ser ""(.*)""")]
    public void ThenLaRazonDeCancelacionDebeSer(string expectedReason)
    {
        _cycle.AbortReason.Should().Be(expectedReason);
    }
}