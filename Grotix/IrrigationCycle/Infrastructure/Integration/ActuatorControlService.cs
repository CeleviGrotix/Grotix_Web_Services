using System.Text;
using System.Text.Json;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.IrrigationCycle.Application.ACL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class ActuatorControlService(
    HardwareDeviceDbContext hardwareDb,
    IActuatorCommandOrchestrator actuatorCommandOrchestrator,
    ILogger<ActuatorControlService> logger) : IActuatorControlService
{
    public async Task<bool> TryActivateIrrigationAsync(int zoneId, CancellationToken cancellationToken = default)
    {
        var actuator = await FindIrrigationActuatorAsync(zoneId, cancellationToken);
        if (actuator == null)
        {
            logger.LogWarning("No irrigation actuator found for zone {ZoneId}", zoneId);
            return false;
        }

        actuator.SetOpen();
        await hardwareDb.SaveChangesAsync(cancellationToken);
        await actuatorCommandOrchestrator.EnqueueAndPublishAsync(zoneId, actuator, "OPEN", cancellationToken);
        return true;
    }

    public async Task TryDeactivateIrrigationAsync(int zoneId, CancellationToken cancellationToken = default)
    {
        var actuator = await FindIrrigationActuatorAsync(zoneId, cancellationToken);
        if (actuator == null)
            return;

        actuator.SetClosed();
        await hardwareDb.SaveChangesAsync(cancellationToken);
        await actuatorCommandOrchestrator.EnqueueAndPublishAsync(zoneId, actuator, "CLOSE", cancellationToken);
    }

    private async Task<DeviceActuator?> FindIrrigationActuatorAsync(
        int zoneId,
        CancellationToken cancellationToken)
    {
        var deviceIds = await hardwareDb.Microcontrollers.AsNoTracking()
            .Where(m => m.ZoneId == zoneId)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (deviceIds.Count == 0)
            return null;

        var allowedTypes = ActuatorTypes.All.ToArray();

        return await hardwareDb.Actuators
            .Where(a => deviceIds.Contains(a.MicrocontrollerId))
            .Where(a => allowedTypes.Contains(a.Type))
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

}
