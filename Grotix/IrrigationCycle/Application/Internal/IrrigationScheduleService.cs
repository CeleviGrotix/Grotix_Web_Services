using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Repositories;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public sealed class IrrigationScheduleService(
    IIrrigationScheduleRepository scheduleRepository,
    IIrrigationUnitOfWork unitOfWork,
    IZoneAccessService zoneAccessService) : IIrrigationScheduleService
{
    public Task<IReadOnlyList<IrrigationSchedule>> ListAsync(int? zoneId) =>
        scheduleRepository.ListAsync(zoneId);

    public async Task<IrrigationSchedule> CreateAsync(
        CreateScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await zoneAccessService.ZoneExistsAsync(request.ZoneId))
            throw new ArgumentException("La zona no existe.");

        var schedule = new IrrigationSchedule(
            request.ZoneId,
            request.DaysOfTheWeek,
            request.StartTime,
            request.DurationMinutes);

        await scheduleRepository.AddAsync(schedule);
        await unitOfWork.CompleteAsync(cancellationToken);
        return schedule;
    }

    public async Task UpdateAsync(
        int scheduleId,
        UpdateScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(scheduleId)
            ?? throw new KeyNotFoundException($"Programa {scheduleId} no encontrado.");

        schedule.Update(request.DaysOfTheWeek, request.StartTime, request.DurationMinutes, request.IsActive);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task DeleteAsync(int scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(scheduleId)
            ?? throw new KeyNotFoundException($"Programa {scheduleId} no encontrado.");

        await scheduleRepository.DeleteAsync(schedule);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
