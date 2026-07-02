using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public interface IIrrigationStartedPublisher
{
    void Publish(IrrigationCycleRecord cycle);
}
