using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Model.ValueObjects;
using GrotixBackend.IrrigationCycle.Domain.Repositories;
using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Repositories;

public sealed class IrrigationCycleRepository(IrrigationCycleDbContext db) : IIrrigationCycleRepository
{
    public Task<IrrigationCycleRecord?> GetByIdAsync(int id) =>
        db.Cycles.FirstOrDefaultAsync(c => c.Id == id);

    public Task<IrrigationCycleRecord?> GetActiveByZoneAsync(int zoneId) =>
        db.Cycles.FirstOrDefaultAsync(c =>
            c.ZoneId == zoneId && c.Status == CycleStatuses.InProgress);

    public async Task<IReadOnlyList<IrrigationCycleRecord>> ListActiveAsync(int? zoneId)
    {
        var query = db.Cycles.AsNoTracking()
            .Where(c => c.Status == CycleStatuses.InProgress);
        if (zoneId.HasValue)
            query = query.Where(c => c.ZoneId == zoneId);
        return await query.OrderByDescending(c => c.StartTime).ToListAsync();
    }

    public async Task<IReadOnlyList<IrrigationCycleRecord>> ListHistoryAsync(
        int? zoneId,
        DateTime? startTime,
        DateTime? endTime,
        int limit)
    {
        var query = db.Cycles.AsNoTracking().AsQueryable();
        if (zoneId.HasValue)
            query = query.Where(c => c.ZoneId == zoneId);
        if (startTime.HasValue)
            query = query.Where(c => c.StartTime >= startTime);
        if (endTime.HasValue)
            query = query.Where(c => c.StartTime <= endTime);

        return await query
            .OrderByDescending(c => c.StartTime)
            .Take(Math.Clamp(limit, 1, 500))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<IrrigationCycleRecord>> ListDueForCompletionAsync(DateTime utcNow)
    {
        var active = await db.Cycles
            .Where(c => c.Status == CycleStatuses.InProgress)
            .ToListAsync();
        return active.Where(c => c.PlannedEndUtc() <= utcNow).ToList();
    }

    public async Task AddAsync(IrrigationCycleRecord cycle) => await db.Cycles.AddAsync(cycle);
}
