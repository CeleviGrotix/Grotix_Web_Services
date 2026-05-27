using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public interface IIrrigationCompletedPublisher
{
    void Publish(IrrigationCycleRecord cycle);
}
