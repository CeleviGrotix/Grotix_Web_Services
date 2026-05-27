using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Application.Internal.Alerting;

public interface IAlertEvaluationService
{
    Task EvaluateAsync(
        Sensor sensor,
        double smoothedValue,
        DateTime timestamp,
        CancellationToken cancellationToken = default);
}
