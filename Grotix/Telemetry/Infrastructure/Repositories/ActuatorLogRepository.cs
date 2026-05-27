using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Telemetry.Infrastructure.Repositories;

public sealed class ActuatorLogRepository(TelemetryDbContext db) : IActuatorLogRepository
{
    public async Task AddAsync(ActuatorLogEntry entry, CancellationToken cancellationToken = default) =>
        await db.ActuatorLogs.AddAsync(entry, cancellationToken);

    public async Task<IReadOnlyList<ActuatorLogEntry>> ListByActuatorAsync(
        int actuatorId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await db.ActuatorLogs.AsNoTracking()
            .Where(l => l.ActuatorId == actuatorId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
