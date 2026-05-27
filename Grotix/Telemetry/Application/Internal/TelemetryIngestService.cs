using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Services;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class TelemetryIngestService(
    ISensorRepository sensorRepository,
    ISensorReadingRepository sensorReadingRepository,
    IAlertEvaluationService alertEvaluationService) : ITelemetryIngestService
{
    public async Task IngestAsync(TelemetryReceivedIntegrationEvent evt, CancellationToken cancellationToken = default)
    {
        var sensor = await sensorRepository.GetByIdAsync(evt.SensorId, cancellationToken);
        if (sensor == null)
            return;

        if (!ReadingRangeValidator.IsPhysicallyValid(sensor, evt.Value))
            return;

        var recent = await sensorReadingRepository.ListBySensorAsync(
            evt.SensorId,
            start: DateTime.UtcNow.AddHours(-1),
            end: null,
            limit: 5,
            cancellationToken);

        var recentValues = recent.Select(r => r.Value).ToList();
        var smoothed = MovingAverageFilter.Smooth(recentValues, evt.Value);

        var reading = new SensorReading
        {
            SensorId = evt.SensorId,
            Value = smoothed,
            Timestamp = evt.Timestamp.ToUniversalTime()
        };

        await sensorReadingRepository.AddAsync(reading, cancellationToken);

        await alertEvaluationService.EvaluateAsync(sensor, smoothed, reading.Timestamp, cancellationToken);
    }
}
