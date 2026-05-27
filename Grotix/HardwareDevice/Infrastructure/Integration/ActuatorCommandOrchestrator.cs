using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class ActuatorCommandOrchestrator(
    HardwareDeviceDbContext hardwareDb,
    IActionQueueRepository actionQueueRepository,
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options,
    ILogger<ActuatorCommandOrchestrator> logger) : IActuatorCommandOrchestrator
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task EnqueueAndPublishAsync(
        int zoneId,
        DeviceActuator actuator,
        string command,
        CancellationToken cancellationToken = default)
    {
        var queueItem = new ActionQueueItem(actuator.Id, command);
        await actionQueueRepository.AddAsync(queueItem, cancellationToken);
        await hardwareDb.SaveChangesAsync(cancellationToken);

        if (!_options.Enabled)
        {
            logger.LogWarning(
                "RabbitMQ disabled; action_queue item {ActionId} stays PENDING.",
                queueItem.Id);
            return;
        }

        PublishActuatorCommand(zoneId, actuator, command);
        queueItem.MarkSent();
        await hardwareDb.SaveChangesAsync(cancellationToken);
    }

    private void PublishActuatorCommand(int zoneId, DeviceActuator actuator, string command)
    {
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
            "Actuator command published: zone={ZoneId}, actuator={ActuatorId}, command={Command}",
            zoneId,
            actuator.Id,
            command);
    }
}
