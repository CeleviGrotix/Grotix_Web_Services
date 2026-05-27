using GrotixBackend.Contracts.Integration.Telemetry;

namespace GrotixBackend.Telemetry.Application.Internal;

public interface ITelemetryIngestService
{
    Task IngestAsync(TelemetryReceivedIntegrationEvent evt, CancellationToken cancellationToken = default);
}
