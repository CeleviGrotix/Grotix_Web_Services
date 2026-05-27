using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Repositories;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public sealed class IrrigationQueryService(IIrrigationCycleRepository cycleRepository) : IIrrigationQueryService
{
    public Task<IReadOnlyList<IrrigationCycleRecord>> GetHistoryAsync(
        int? zoneId,
        DateTime? startTime,
        DateTime? endTime,
        int? limit) =>
        cycleRepository.ListHistoryAsync(zoneId, startTime, endTime, limit ?? 50);

    public Task<IReadOnlyList<IrrigationCycleRecord>> GetActiveAsync(int? zoneId) =>
        cycleRepository.ListActiveAsync(zoneId);
}
