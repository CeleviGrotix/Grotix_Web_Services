using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Telemetry.Infrastructure.Repositories;

public sealed class ActiveThresholdRepository(TelemetryDbContext db) : IActiveThresholdRepository
{
    public async Task<IReadOnlyList<ActiveThreshold>> ListByZoneAsync(int zoneId, CancellationToken cancellationToken = default)
    {
        var list = await db.ActiveThresholds.AsNoTracking()
            .Where(t => t.ZoneId == zoneId)
            .ToListAsync(cancellationToken);
        return list;
    }

    public Task<ActiveThreshold?> GetAsync(int zoneId, string sensorType, CancellationToken cancellationToken = default)
    {
        var normalized = SensorTypes.Normalize(sensorType);
        return db.ActiveThresholds.FirstOrDefaultAsync(
            t => t.ZoneId == zoneId && t.SensorType == normalized,
            cancellationToken);
    }

    public async Task UpsertAsync(ActiveThreshold threshold, CancellationToken cancellationToken = default)
    {
        threshold.SensorType = SensorTypes.Normalize(threshold.SensorType);
        var existing = await db.ActiveThresholds
            .FirstOrDefaultAsync(
                t => t.ZoneId == threshold.ZoneId && t.SensorType == threshold.SensorType,
                cancellationToken);

        if (existing == null)
            db.ActiveThresholds.Add(threshold);
        else
        {
            existing.MinValue = threshold.MinValue;
            existing.MaxValue = threshold.MaxValue;
        }
    }

    public async Task DeleteAsync(int zoneId, string sensorType, CancellationToken cancellationToken = default)
    {
        var normalized = SensorTypes.Normalize(sensorType);
        var existing = await db.ActiveThresholds
            .FirstOrDefaultAsync(t => t.ZoneId == zoneId && t.SensorType == normalized, cancellationToken);
        if (existing != null)
            db.ActiveThresholds.Remove(existing);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
