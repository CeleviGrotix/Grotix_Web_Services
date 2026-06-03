using FluentAssertions;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using TechTalk.SpecFlow;

namespace HardwareDevice.Api.Tests.Acceptance.Steps;

[Binding]
public class HardwareSteps
{
    private Microcontroller _device = default!;
    private TechnicalMaintenance _maintenanceRecord = default!;

    // --- US12: Cambio de estado ---
    [Given(@"un dispositivo ESP32 registrado y activo")]
    public void GivenUnDispositivoRegistrado()
    {
        _device = new Microcontroller("ESP32", "MAC_ADDRESS", 1);
        _device.UpdateStatus("ACTIVE");
    }

    [When(@"el administrador cambia su estado a ""(.*)""")]
    public void WhenCambiaEstado(string newState)
    {
        _device.UpdateStatus(newState);
    }

    [Then(@"el sistema debe actualizar el estado del dispositivo exitosamente")]
    public void ThenSistemaActualizaEstado()
    {
        _device.Status.Should().NotBeNullOrEmpty();
    }

    [Then(@"el nuevo estado debe reflejarse como ""(.*)""")]
    public void ThenNuevoEstadoDebeReflejarseComo(string expectedState)
    {
        // Tu sistema asume que el ValueObject normaliza a mayúsculas
        _device.Status.Should().Be(expectedState.ToUpperInvariant());
    }

    // --- US12: Bitácora ---
    [Given(@"un dispositivo en revision")]
    public void GivenUnDispositivoEnRevision()
    {
        _device = new Microcontroller("ESP32", "MAC_ADDRESS", 1);
    }

    [When(@"el tecnico completa la bitacora con la descripcion ""(.*)""")]
    public void WhenTecnicoCompletaBitacora(string desc)
    {
        _maintenanceRecord = new TechnicalMaintenance(10, 1, "Preventivo", desc);
    }

    [Then(@"el sistema debe crear un registro tecnico con la fecha actual")]
    public void ThenSistemaCreaRegistroTecnico()
    {
        _maintenanceRecord.Should().NotBeNull();
        _maintenanceRecord.Date.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Then(@"la descripcion guardada debe ser ""(.*)""")]
    public void ThenDescripcionGuardadaDebeSer(string expectedDesc)
    {
        _maintenanceRecord.Description.Should().Be(expectedDesc);
    }

    // --- US15: Desvinculación ---
    [Given(@"un dispositivo asignado a la zona (.*)")]
    public void GivenDispositivoAsignadoAZona(int zoneId)
    {
        _device = new Microcontroller("ESP32", "MAC", zoneId);
    }

    [When(@"el administrador ejecuta la desvinculacion")]
    public void WhenEjecutaDesvinculacion()
    {
        _device.UnlinkFromZone();
    }

    [Then(@"el dispositivo no debe estar asociado a ninguna zona")]
    public void ThenDispositivoNoDebeEstarAsociado()
    {
        _device.ZoneId.Should().BeNull();
    }
}