using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Domain.Repositories;

public interface IActiveThresholdRepository
{
    Task<IReadOnlyList<ActiveThreshold>> ListByZoneAsync(int zoneId, CancellationToken cancellationToken = default);

    Task<ActiveThreshold?> GetAsync(int zoneId, string sensorType, CancellationToken cancellationToken = default);

    Task UpsertAsync(ActiveThreshold threshold, CancellationToken cancellationToken = default);

    Task DeleteAsync(int zoneId, string sensorType, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
