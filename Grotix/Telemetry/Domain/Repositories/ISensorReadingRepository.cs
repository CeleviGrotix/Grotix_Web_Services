using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Domain.Repositories;

public interface ISensorReadingRepository
{
    Task AddAsync(SensorReading reading, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SensorReading>> ListByZoneAsync(
        int zoneId,
        DateTime? start,
        DateTime? end,
        int limit,
        CancellationToken cancellationToken = default);

    Task<SensorReading?> GetLatestByDeviceAsync(int deviceId, CancellationToken cancellationToken = default);
}
