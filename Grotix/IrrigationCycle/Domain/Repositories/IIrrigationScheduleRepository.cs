using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Domain.Repositories;

public interface IIrrigationScheduleRepository
{
    Task<IrrigationSchedule?> GetByIdAsync(int id);
    Task<IReadOnlyList<IrrigationSchedule>> ListAsync(int? zoneId);
    Task AddAsync(IrrigationSchedule schedule);
    Task DeleteAsync(IrrigationSchedule schedule);
}
