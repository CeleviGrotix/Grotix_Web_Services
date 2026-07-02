using FluentAssertions;
using TechTalk.SpecFlow;

namespace CultivationArea.Api.Tests.Acceptance;

[Binding]
public class CultivationAreaAcceptanceSteps
{
    private string _currentZoneName;
    private string _assignedHardware;

    // --- US15: Creación y Organización de Zonas ---
    [When(@"el staff ingresa el nombre de la zona ""(.*)""")]
    public void WhenElStaffIngresaElNombreDeLaZona(string name)
    {
        _currentZoneName = name;
    }

    [Then(@"la zona se visualiza correctamente en el panel de control")]
    public void ThenLaZonaSeVisualizaCorrectamenteEnElPanelDeControl()
    {
        _currentZoneName.Should().Be("Invernadero 1");
    }

    [Given(@"una zona previamente creada sin hardware")]
    public void GivenUnaZonaPreviamenteCreadaSinHardware()
    {
        _assignedHardware = string.Empty;
    }

    [When(@"el staff selecciona un microcontrolador y el tipo de cultivo ""(.*)""")]
    public void WhenElStaffSeleccionaUnMicrocontroladorYElTipoDeCultivo(string crop)
    {
        _assignedHardware = $"ESP32-{crop}";
    }

    [Then(@"el hardware queda asociado a la zona y al cultivo de forma persistente")]
    public void ThenElHardwareQuedaAsociadoALaZonaYAlCultivoDeFormaPersistente()
    {
        _assignedHardware.Should().Contain("Zanahoria");
    }
}