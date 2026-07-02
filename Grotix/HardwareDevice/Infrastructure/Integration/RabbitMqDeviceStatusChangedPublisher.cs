using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using Microsoft.Extensions.Options;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class RabbitMqDeviceStatusChangedPublisher(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options) : IDeviceStatusChangedPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(Microcontroller device, string oldStatus, string newStatus)
    {
        if (!_options.Enabled)
            return;

        var evt = new DeviceStatusChangedIntegrationEvent(
            device.Id,
            oldStatus,
            newStatus,
            DateTime.UtcNow);

        var json = JsonSerializer.Serialize(evt);
        publisher.TryPublish(_options.DeviceStatusChangedRoutingKey, Encoding.UTF8.GetBytes(json));
    }
}

