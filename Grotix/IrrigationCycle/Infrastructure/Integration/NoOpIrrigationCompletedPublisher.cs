using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class NoOpIrrigationCompletedPublisher : IIrrigationCompletedPublisher
{
    public void Publish(IrrigationCycleRecord cycle) { }
}
