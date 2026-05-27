using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public interface IIrrigationScheduleService
{
    Task<IReadOnlyList<IrrigationSchedule>> ListAsync(int? zoneId);
    Task<IrrigationSchedule> CreateAsync(CreateScheduleRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int scheduleId, UpdateScheduleRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int scheduleId, CancellationToken cancellationToken = default);
}

public sealed record CreateScheduleRequest(
    int ZoneId,
    string DaysOfTheWeek,
    TimeOnly StartTime,
    int DurationMinutes);

public sealed record UpdateScheduleRequest(
    string? DaysOfTheWeek,
    TimeOnly? StartTime,
    int? DurationMinutes,
    bool? IsActive);
