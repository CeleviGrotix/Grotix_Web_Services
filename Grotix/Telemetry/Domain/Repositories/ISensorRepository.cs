using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Domain.Repositories;

public interface ISensorRepository
{
    Task<Sensor?> GetByIdAsync(int sensorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sensor>> ListByZoneAsync(int zoneId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sensor>> ListByZoneAndTypesAsync(
        int zoneId,
        IReadOnlyCollection<string> sensorTypes,
        CancellationToken cancellationToken = default);
}
