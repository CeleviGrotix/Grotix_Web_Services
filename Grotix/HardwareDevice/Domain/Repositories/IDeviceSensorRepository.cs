using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Domain.Repositories;

public interface IDeviceSensorRepository
{
    Task<IReadOnlyList<DeviceSensor>> ListByDeviceAsync(int deviceId);
    Task<int> CountByDeviceAsync(int deviceId);
    Task AddAsync(DeviceSensor sensor);

    Task AssignZoneToDeviceAsync(int deviceId, int? zoneId, CancellationToken cancellationToken = default);
}
