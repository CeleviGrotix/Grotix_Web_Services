using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IActuatorCommandOrchestrator
{
    Task EnqueueAndPublishAsync(
        int zoneId,
        DeviceActuator actuator,
        string command,
        CancellationToken cancellationToken = default);
}
