using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public interface IIrrigationQueryService
{
    Task<IReadOnlyList<IrrigationCycleRecord>> GetHistoryAsync(
        int? zoneId,
        DateTime? startTime,
        DateTime? endTime,
        int? limit);

    Task<IReadOnlyList<IrrigationCycleRecord>> GetActiveAsync(int? zoneId);
}
