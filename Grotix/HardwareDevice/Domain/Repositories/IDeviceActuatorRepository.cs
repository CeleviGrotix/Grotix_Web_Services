using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Domain.Repositories;

public interface IDeviceActuatorRepository
{
    Task<IReadOnlyList<DeviceActuator>> ListByDeviceAsync(int deviceId);
    Task<int> CountByDeviceAsync(int deviceId);
}
