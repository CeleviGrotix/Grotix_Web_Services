using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class AlertQueryService(IAlertRecordRepository alertRecordRepository) : IAlertQueryService
{
    public Task<IReadOnlyList<AlertRecord>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default) =>
        alertRecordRepository.ListByZoneAsync(zoneId, Math.Clamp(limit, 1, 500), cancellationToken);
}
