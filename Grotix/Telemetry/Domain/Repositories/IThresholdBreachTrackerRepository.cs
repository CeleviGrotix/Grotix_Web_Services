using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Domain.Repositories;

public interface IThresholdBreachTrackerRepository
{
    Task<ThresholdBreachTracker?> GetAsync(int zoneId, int sensorId, CancellationToken cancellationToken = default);

    Task UpsertAsync(ThresholdBreachTracker tracker, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
