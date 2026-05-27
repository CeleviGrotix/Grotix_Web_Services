using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Repositories;

public sealed class DeviceActuatorRepository(HardwareDeviceDbContext db) : IDeviceActuatorRepository
{
    public async Task<IReadOnlyList<DeviceActuator>> ListByDeviceAsync(int deviceId)
    {
        var list = await db.Actuators.AsNoTracking()
            .Where(a => a.MicrocontrollerId == deviceId)
            .OrderBy(a => a.Id)
            .ToListAsync();
        return list;
    }

    public Task<int> CountByDeviceAsync(int deviceId) =>
        db.Actuators.CountAsync(a => a.MicrocontrollerId == deviceId);

    public Task<DeviceActuator?> GetByIdAsync(int actuatorId) =>
        db.Actuators.FirstOrDefaultAsync(a => a.Id == actuatorId);

    public async Task AddAsync(DeviceActuator actuator) => await db.Actuators.AddAsync(actuator);

    public Task DeleteAsync(DeviceActuator actuator)
    {
        db.Actuators.Remove(actuator);
        return Task.CompletedTask;
    }
}
