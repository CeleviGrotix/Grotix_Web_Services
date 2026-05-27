using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Repositories;

public sealed class TechnicalMaintenanceRepository(HardwareDeviceDbContext db) : ITechnicalMaintenanceRepository
{
    public async Task AddAsync(TechnicalMaintenance maintenance, CancellationToken cancellationToken = default) =>
        await db.TechnicalMaintenances.AddAsync(maintenance, cancellationToken);

    public async Task<IReadOnlyList<TechnicalMaintenance>> ListByDeviceAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.TechnicalMaintenances.AsNoTracking()
            .Where(m => m.DeviceId == deviceId)
            .OrderByDescending(m => m.Date)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TechnicalMaintenance>> ListByStaffAsync(
        int staffId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.TechnicalMaintenances.AsNoTracking()
            .Where(m => m.StaffId == staffId)
            .OrderByDescending(m => m.Date)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
