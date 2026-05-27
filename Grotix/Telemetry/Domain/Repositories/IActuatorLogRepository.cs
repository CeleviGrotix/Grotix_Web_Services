using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Domain.Repositories;

public interface IActuatorLogRepository
{
    Task AddAsync(ActuatorLogEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActuatorLogEntry>> ListByActuatorAsync(
        int actuatorId,
        int limit,
        CancellationToken cancellationToken = default);
}
