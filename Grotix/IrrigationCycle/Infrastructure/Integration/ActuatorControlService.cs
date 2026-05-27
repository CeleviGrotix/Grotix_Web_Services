using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.IrrigationCycle.Application.ACL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class ActuatorControlService(
    HardwareDeviceDbContext hardwareDb,
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options,
    ILogger<ActuatorControlService> logger) : IActuatorControlService
{
    private static readonly string[] IrrigationActuatorTypes = ["VALVE", "PUMP", "IRRIGATION"];
    private readonly RabbitMqOptions _options = options.Value;

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
        PublishActuatorCommand(zoneId, actuator, command: "OPEN");
        return true;
    }

    public async Task TryDeactivateIrrigationAsync(int zoneId, CancellationToken cancellationToken = default)
    {
        var actuator = await FindIrrigationActuatorAsync(zoneId, cancellationToken);
        if (actuator == null)
            return;

        actuator.SetClosed();
        await hardwareDb.SaveChangesAsync(cancellationToken);
        PublishActuatorCommand(zoneId, actuator, command: "CLOSE");
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

        return await hardwareDb.Actuators
            .Where(a => deviceIds.Contains(a.MicrocontrollerId))
            .Where(a =>
                a.Type.Contains("VALVE") ||
                a.Type.Contains("PUMP") ||
                a.Type.Contains("IRRIGATION"))
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void PublishActuatorCommand(int zoneId, DeviceActuator actuator, string command)
    {
        if (!_options.Enabled)
            return;

        var evt = new ActuatorCommandIntegrationEvent(
            zoneId,
            actuator.Id,
            actuator.MicrocontrollerId,
            actuator.Type,
            actuator.Pin,
            command,
            DateTime.UtcNow);

        var json = JsonSerializer.Serialize(evt);
        publisher.Publish(_options.ActuatorCommandRoutingKey, Encoding.UTF8.GetBytes(json));

        logger.LogInformation(
            "Actuator command published: zone={ZoneId}, actuator={ActuatorId}, command={Command}, routingKey={RoutingKey}",
            zoneId,
            actuator.Id,
            command,
            _options.ActuatorCommandRoutingKey);
    }
}
