using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Telemetry.Infrastructure.Repositories;

public sealed class ThresholdBreachTrackerRepository(TelemetryDbContext db) : IThresholdBreachTrackerRepository
{
    public Task<ThresholdBreachTracker?> GetAsync(
        int zoneId,
        int sensorId,
        CancellationToken cancellationToken = default) =>
        db.ThresholdBreachTrackers.FirstOrDefaultAsync(
            t => t.ZoneId == zoneId && t.SensorId == sensorId,
            cancellationToken);

    public async Task UpsertAsync(ThresholdBreachTracker tracker, CancellationToken cancellationToken = default)
    {
        var existing = await db.ThresholdBreachTrackers
            .FirstOrDefaultAsync(
                t => t.ZoneId == tracker.ZoneId && t.SensorId == tracker.SensorId,
                cancellationToken);

        if (existing == null)
            db.ThresholdBreachTrackers.Add(tracker);
        else
        {
            existing.SensorType = tracker.SensorType;
            existing.ConsecutiveCount = tracker.ConsecutiveCount;
            existing.LastEvaluatedAt = tracker.LastEvaluatedAt;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
