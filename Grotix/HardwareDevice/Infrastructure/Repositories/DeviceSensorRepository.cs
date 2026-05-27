using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Repositories;

public sealed class DeviceSensorRepository(HardwareDeviceDbContext db) : IDeviceSensorRepository
{
    public async Task<IReadOnlyList<DeviceSensor>> ListByDeviceAsync(int deviceId)
    {
        var list = await db.Sensors.AsNoTracking()
            .Where(s => s.MicrocontrollerId == deviceId)
            .OrderBy(s => s.Id)
            .ToListAsync();
        return list;
    }

    public Task<int> CountByDeviceAsync(int deviceId) =>
        db.Sensors.CountAsync(s => s.MicrocontrollerId == deviceId);

    public async Task AddAsync(DeviceSensor sensor) => await db.Sensors.AddAsync(sensor);

    public Task<DeviceSensor?> GetByIdAsync(int sensorId) =>
        db.Sensors.FirstOrDefaultAsync(s => s.Id == sensorId);

    public Task DeleteAsync(DeviceSensor sensor)
    {
        db.Sensors.Remove(sensor);
        return Task.CompletedTask;
    }

    public async Task AssignZoneToDeviceAsync(int deviceId, int? zoneId, CancellationToken cancellationToken = default)
    {
        var sensors = await db.Sensors.Where(s => s.MicrocontrollerId == deviceId).ToListAsync(cancellationToken);
        foreach (var sensor in sensors)
            sensor.AssignZone(zoneId);
    }
}
