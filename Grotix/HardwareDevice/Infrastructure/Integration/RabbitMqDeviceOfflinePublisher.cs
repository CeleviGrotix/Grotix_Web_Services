using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using Microsoft.Extensions.Options;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class RabbitMqDeviceOfflinePublisher(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options) : IDeviceOfflinePublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(Microcontroller device)
    {
        if (!_options.Enabled)
            return;

        var evt = new DeviceOfflineIntegrationEvent(
            device.Id,
            device.ZoneId,
            DateTime.UtcNow);

        var json = JsonSerializer.Serialize(evt);
        publisher.TryPublish(_options.DeviceOfflineRoutingKey, Encoding.UTF8.GetBytes(json));
    }
}
