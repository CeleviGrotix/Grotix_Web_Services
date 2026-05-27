using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Telemetry.Infrastructure.Repositories;

public sealed class SensorRepository(TelemetryDbContext db) : ISensorRepository
{
    public Task<Sensor?> GetByIdAsync(int sensorId, CancellationToken cancellationToken = default) =>
        db.Sensors.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sensorId, cancellationToken);

    public async Task<IReadOnlyList<Sensor>> ListByZoneAsync(int zoneId, CancellationToken cancellationToken = default)
    {
        var list = await db.Sensors.AsNoTracking()
            .Where(s => s.ZoneId == zoneId)
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task<IReadOnlyList<Sensor>> ListByZoneAndTypesAsync(
        int zoneId,
        IReadOnlyCollection<string> sensorTypes,
        CancellationToken cancellationToken = default)
    {
        var normalized = sensorTypes.Select(SensorTypes.Normalize).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var list = await db.Sensors.AsNoTracking()
            .Where(s => s.ZoneId == zoneId && normalized.Contains(s.Type))
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
        return list;
    }
}
