using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Repositories;
using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Repositories;

public sealed class IrrigationScheduleRepository(IrrigationCycleDbContext db) : IIrrigationScheduleRepository
{
    public Task<IrrigationSchedule?> GetByIdAsync(int id) =>
        db.Schedules.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IReadOnlyList<IrrigationSchedule>> ListAsync(int? zoneId)
    {
        var query = db.Schedules.AsNoTracking().AsQueryable();
        if (zoneId.HasValue)
            query = query.Where(s => s.ZoneId == zoneId);
        return await query.OrderBy(s => s.ZoneId).ThenBy(s => s.StartTime).ToListAsync();
    }

    public async Task AddAsync(IrrigationSchedule schedule) => await db.Schedules.AddAsync(schedule);

    public Task DeleteAsync(IrrigationSchedule schedule)
    {
        db.Schedules.Remove(schedule);
        return Task.CompletedTask;
    }
}
