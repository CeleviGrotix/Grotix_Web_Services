using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Irrigation;
using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using Microsoft.Extensions.Options;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class RabbitMqIrrigationStartedPublisher(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options) : IIrrigationStartedPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(IrrigationCycleRecord cycle)
    {
        if (!_options.Enabled)
            return;

        var evt = new IrrigationStartedIntegrationEvent(
            cycle.Id,
            cycle.ZoneId,
            cycle.VolumeLiters,
            cycle.DurationMinutes,
            cycle.StartTime);

        var json = JsonSerializer.Serialize(evt);
        publisher.TryPublish(_options.IrrigationStartedRoutingKey, Encoding.UTF8.GetBytes(json));
    }
}
