using GrotixBackend.HardwareDevice.Domain.Model.Entities;

namespace GrotixBackend.HardwareDevice.Domain.Repositories;

public interface IMaintenanceLogRepository
{
    Task AddAsync(MaintenanceLog log, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenanceLog>> ListByDeviceAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default);
}
