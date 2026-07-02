using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Irrigation;
using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Model.ValueObjects;
using Microsoft.Extensions.Options;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class RabbitMqIrrigationCompletedPublisher(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options) : IIrrigationCompletedPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(IrrigationCycleRecord cycle)
    {
        if (!_options.Enabled)
            return;

        var evt = new IrrigationCompletedIntegrationEvent(
            cycle.Id,
            cycle.ZoneId,
            cycle.VolumeLiters,
            CycleStatuses.ToApiStatus(cycle.Status),
            cycle.EndTime ?? DateTime.UtcNow);

        var json = JsonSerializer.Serialize(evt);
        publisher.TryPublish(_options.IrrigationCompletedRoutingKey, Encoding.UTF8.GetBytes(json));
    }
}
