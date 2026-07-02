using FluentAssertions;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using TechTalk.SpecFlow;

namespace CultivationArea.Api.Tests.Acceptance.Steps;

[Binding]
public class CultivationAreaSteps
{
    private int _farmId;
    private Zone _zone = default!;

    // --- Escenario 1: Creación de zona ---
    [Given(@"una granja existente con ID (.*)")]
    public void GivenUnaGranjaExistenteConID(int farmId)
    {
        _farmId = farmId;
    }

    [When(@"el usuario crea una zona para el cultivo (.*) en las coordenadas (.*) y (.*)")]
    public void WhenElUsuarioCreaUnaZona(int cropId, double lat, double lng)
    {
        _zone = new Zone(_farmId, cropId, lat, lng);
    }

    [Then(@"el sistema debe guardar la zona exitosamente en la granja (.*)")]
    public void ThenElSistemaDebeGuardarLaZonaEnLaGranja(int farmId)
    {
        _zone.Should().NotBeNull();
        _zone.FarmId.Should().Be(farmId);
    }

    [Then(@"la latitud de la zona debe ser (.*)")]
    public void ThenLaLatitudDeLaZonaDebeSer(double lat)
    {
        _zone.Latitude.Should().Be(lat);
    }

    // --- Escenario 2: Reasignación de Especie ---
    [Given(@"una zona de cultivo registrada con la especie (.*)")]
    public void GivenUnaZonaRegistradaConLaEspecie(int cropId)
    {
        _zone = new Zone(1, cropId, 10.0, 10.0);
    }

    [When(@"el usuario modifica la zona para sembrar la especie (.*)")]
    public void WhenElUsuarioModificaLaZonaParaSembrar(int newCropId)
    {
        _zone.ReassignCrop(newCropId);
    }

    [Then(@"el identificador de la especie en la zona debe actualizarse a (.*)")]
    public void ThenElIdentificadorDeLaEspecieDebeActualizarse(int expectedCropId)
    {
        _zone.CropId.Should().Be(expectedCropId);
    }

    // --- Escenario 3: Actualización de Fase ---
    [Given(@"una zona de cultivo en fase ""(.*)""")]
    public void GivenUnaZonaEnFase(string phase)
    {
        _zone = new Zone(1, 2, 10.0, 10.0, phase, DateTime.UtcNow);
    }

    [When(@"el usuario actualiza la fase a ""(.*)""")]
    public void WhenElUsuarioActualizaLaFaseA(string newPhase)
    {
        _zone.UpdatePhase(newPhase, DateTime.UtcNow);
    }

    [Then(@"el sistema debe registrar el estado actual como ""(.*)""")]
    public void ThenElSistemaDebeRegistrarElEstadoActualComo(string expectedPhase)
    {
        _zone.CurrentPhase.Should().Be(expectedPhase);
    }
}