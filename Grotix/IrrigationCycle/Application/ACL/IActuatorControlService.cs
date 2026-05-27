namespace GrotixBackend.IrrigationCycle.Application.ACL;

public interface IActuatorControlService
{
    Task<bool> TryActivateIrrigationAsync(int zoneId, CancellationToken cancellationToken = default);
    Task TryDeactivateIrrigationAsync(int zoneId, CancellationToken cancellationToken = default);
}
