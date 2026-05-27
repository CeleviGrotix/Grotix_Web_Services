using GrotixBackend.HardwareDevice.Domain.Model.Entities;

namespace GrotixBackend.HardwareDevice.Domain.Repositories;

public interface IActionQueueRepository
{
    Task AddAsync(ActionQueueItem item, CancellationToken cancellationToken = default);

    Task<ActionQueueItem?> FindLatestPendingByActuatorAsync(
        int actuatorId,
        string command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActionQueueItem>> ListByActuatorAsync(
        int actuatorId,
        int limit,
        CancellationToken cancellationToken = default);
}
