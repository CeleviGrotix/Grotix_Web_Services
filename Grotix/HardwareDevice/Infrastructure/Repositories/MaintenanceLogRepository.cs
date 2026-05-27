using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Repositories;

public sealed class MaintenanceLogRepository(HardwareDeviceDbContext db) : IMaintenanceLogRepository
{
    public async Task AddAsync(MaintenanceLog log, CancellationToken cancellationToken = default) =>
        await db.MaintenanceLogs.AddAsync(log, cancellationToken);

    public async Task<IReadOnlyList<MaintenanceLog>> ListByDeviceAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.MaintenanceLogs.AsNoTracking()
            .Where(m => m.DeviceId == deviceId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
