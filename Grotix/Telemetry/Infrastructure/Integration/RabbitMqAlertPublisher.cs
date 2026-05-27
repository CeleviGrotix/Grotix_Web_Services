using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using Microsoft.Extensions.Options;

namespace GrotixBackend.Telemetry.Infrastructure.Integration;

public sealed class RabbitMqAlertPublisher(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options) : IAlertPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(AlertTriggeredIntegrationEvent alert)
    {
        var json = JsonSerializer.Serialize(alert);
        publisher.Publish(_options.AlertTriggeredRoutingKey, System.Text.Encoding.UTF8.GetBytes(json));
    }
}
