using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Telemetry.Infrastructure.Repositories;

public sealed class SensorReadingRepository(TelemetryDbContext db) : ISensorReadingRepository
{
    public async Task AddAsync(SensorReading reading, CancellationToken cancellationToken = default)
    {
        db.SensorReadings.Add(reading);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SensorReading>> ListBySensorAsync(
        int sensorId,
        DateTime? start,
        DateTime? end,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = db.SensorReadings.AsNoTracking().Where(r => r.SensorId == sensorId);
        if (start.HasValue)
            query = query.Where(r => r.Timestamp >= start.Value);
        if (end.HasValue)
            query = query.Where(r => r.Timestamp <= end.Value);

        return await query
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<double?> GetLatestSmoothedValueAsync(int sensorId, CancellationToken cancellationToken = default)
    {
        var reading = await db.SensorReadings.AsNoTracking()
            .Where(r => r.SensorId == sensorId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => (double?)r.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return reading;
    }
}
