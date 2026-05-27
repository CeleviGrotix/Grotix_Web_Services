using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public interface IIrrigationCommandService
{
    Task<IrrigationCycleRecord> StartManualAsync(
        int zoneId,
        double? volumeLiters,
        int? durationMinutes,
        CancellationToken cancellationToken = default);

    Task CompleteCycleAsync(int cycleId, CancellationToken cancellationToken = default);
}
