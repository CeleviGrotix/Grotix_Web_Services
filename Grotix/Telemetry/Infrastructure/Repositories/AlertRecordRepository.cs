using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Telemetry.Infrastructure.Repositories;

public sealed class AlertRecordRepository(TelemetryDbContext db) : IAlertRecordRepository
{
    public async Task AddAsync(AlertRecord alert, CancellationToken cancellationToken = default) =>
        await db.AlertRecords.AddAsync(alert, cancellationToken);

    public async Task<IReadOnlyList<AlertRecord>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.AlertRecords.AsNoTracking()
            .Where(a => a.ZoneId == zoneId)
            .OrderByDescending(a => a.TriggeredAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
