using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Repositories;

public sealed class MicrocontrollerRepository(HardwareDeviceDbContext db) : IMicrocontrollerRepository
{
    public Task<Microcontroller?> GetByIdAsync(int id) =>
        db.Microcontrollers.FirstOrDefaultAsync(d => d.Id == id);

    public Task<Microcontroller?> GetByMacAddressAsync(string macAddress)
    {
        var normalized = macAddress.Trim().ToUpperInvariant();
        return db.Microcontrollers.FirstOrDefaultAsync(d => d.MacAddress == normalized);
    }

    public async Task<IReadOnlyList<Microcontroller>> ListAsync(string? status, int? zoneId)
    {
        var query = db.Microcontrollers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = DeviceStatuses.Normalize(status);
            query = query.Where(d => d.Status == normalized);
        }

        if (zoneId.HasValue)
            query = query.Where(d => d.ZoneId == zoneId);

        return await query.OrderBy(d => d.Id).ToListAsync();
    }

    public Task<IReadOnlyList<Microcontroller>> ListByZoneAsync(int zoneId) =>
        ListAsync(status: null, zoneId: zoneId);

    public async Task<IReadOnlyList<Microcontroller>> ListOnlineStaleAsync(
        DateTime lastSeenBefore,
        CancellationToken cancellationToken = default)
    {
        var devices = await db.Microcontrollers
            .Where(d =>
                d.Status == DeviceStatuses.Online &&
                d.LastSeen != null &&
                d.LastSeen < lastSeenBefore)
            .ToListAsync(cancellationToken);

        return devices;
    }

    public async Task AddAsync(Microcontroller device)
    {
        await db.Microcontrollers.AddAsync(device);
    }

    public Task DeleteAsync(Microcontroller device)
    {
        db.Microcontrollers.Remove(device);
        return Task.CompletedTask;
    }
}
