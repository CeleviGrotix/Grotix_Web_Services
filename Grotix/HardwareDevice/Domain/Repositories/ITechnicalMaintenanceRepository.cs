using GrotixBackend.HardwareDevice.Domain.Model.Entities;

namespace GrotixBackend.HardwareDevice.Domain.Repositories;

public interface ITechnicalMaintenanceRepository
{
    Task AddAsync(TechnicalMaintenance maintenance, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TechnicalMaintenance>> ListByDeviceAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TechnicalMaintenance>> ListByStaffAsync(
        int staffId,
        int limit,
        CancellationToken cancellationToken = default);
}
