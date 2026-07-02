using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.Telemetry.Application.Internal;
using Microsoft.Extensions.Options;

namespace GrotixBackend.Telemetry.Infrastructure.Integration;

public sealed class RabbitMqDeviceHeartbeatPublisher(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options) : IDeviceHeartbeatPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(DeviceHeartbeatIntegrationEvent heartbeat)
    {
        if (!_options.Enabled)
            return;

        var json = JsonSerializer.Serialize(heartbeat);
        publisher.TryPublish(_options.DeviceHeartbeatRoutingKey, Encoding.UTF8.GetBytes(json));
    }
}
