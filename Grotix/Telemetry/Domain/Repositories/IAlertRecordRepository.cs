using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Domain.Repositories;

public interface IAlertRecordRepository
{
    Task AddAsync(AlertRecord alert, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertRecord>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default);
}
