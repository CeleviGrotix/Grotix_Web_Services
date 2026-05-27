using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Repositories;

public sealed class ActionQueueRepository(HardwareDeviceDbContext db) : IActionQueueRepository
{
    public async Task AddAsync(ActionQueueItem item, CancellationToken cancellationToken = default) =>
        await db.ActionQueue.AddAsync(item, cancellationToken);

    public async Task<ActionQueueItem?> FindLatestPendingByActuatorAsync(
        int actuatorId,
        string command,
        CancellationToken cancellationToken = default)
    {
        var normalized = command.Trim().ToUpperInvariant();
        return await db.ActionQueue
            .Where(a => a.ActuatorId == actuatorId
                        && a.Command == normalized
                        && (a.Status == ActionQueueItem.StatusPending || a.Status == ActionQueueItem.StatusSent))
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActionQueueItem>> ListByActuatorAsync(
        int actuatorId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.ActionQueue.AsNoTracking()
            .Where(a => a.ActuatorId == actuatorId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
