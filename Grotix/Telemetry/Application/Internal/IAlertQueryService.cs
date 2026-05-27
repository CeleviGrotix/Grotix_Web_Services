using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Application.Internal;

public interface IAlertQueryService
{
    Task<IReadOnlyList<AlertRecord>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default);
}
