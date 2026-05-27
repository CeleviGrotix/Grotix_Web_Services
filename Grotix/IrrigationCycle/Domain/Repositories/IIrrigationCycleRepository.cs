using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Domain.Repositories;

public interface IIrrigationCycleRepository
{
    Task<IrrigationCycleRecord?> GetByIdAsync(int id);
    Task<IrrigationCycleRecord?> GetActiveByZoneAsync(int zoneId);
    Task<IReadOnlyList<IrrigationCycleRecord>> ListActiveAsync(int? zoneId = null);
    Task<IReadOnlyList<IrrigationCycleRecord>> ListHistoryAsync(
        int? zoneId,
        DateTime? startTime,
        DateTime? endTime,
        int limit);
    Task<IReadOnlyList<IrrigationCycleRecord>> ListDueForCompletionAsync(DateTime utcNow);
    Task AddAsync(IrrigationCycleRecord cycle);
}
