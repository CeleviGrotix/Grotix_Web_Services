using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.ACL;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Services;
using Microsoft.Extensions.Options;

namespace GrotixBackend.Telemetry.Application.Internal.Alerting;

public sealed class AlertEvaluationService(
    IEffectiveThresholdResolver thresholdResolver,
    IThresholdBreachTrackerRepository breachTrackerRepository,
    IAlertRecordRepository alertRecordRepository,
    IAlertPublisher alertPublisher,
    IOptions<AlertEvaluationOptions> options) : IAlertEvaluationService
{
    private readonly AlertEvaluationOptions _options = options.Value;

    public async Task EvaluateAsync(
        Sensor sensor,
        double smoothedValue,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var threshold = await thresholdResolver.ResolveForSensorAsync(sensor.ZoneId, sensor.Type, cancellationToken);
        if (threshold == null)
            return;

        var breached = ThresholdEvaluator.IsOutOfRange(smoothedValue, threshold.MinValue, threshold.MaxValue);
        var tracker = await breachTrackerRepository.GetAsync(sensor.ZoneId, sensor.Id, cancellationToken)
            ?? new ThresholdBreachTracker
            {
                ZoneId = sensor.ZoneId,
                SensorId = sensor.Id,
                SensorType = sensor.Type,
                ConsecutiveCount = 0
            };

        tracker.LastEvaluatedAt = timestamp;

        if (!breached)
        {
            tracker.ConsecutiveCount = 0;
            await breachTrackerRepository.UpsertAsync(tracker, cancellationToken);
            await breachTrackerRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        tracker.ConsecutiveCount++;
        await breachTrackerRepository.UpsertAsync(tracker, cancellationToken);
        await breachTrackerRepository.SaveChangesAsync(cancellationToken);

        var required = Math.Max(1, _options.SustainedReadingsBeforeAlert);
        if (tracker.ConsecutiveCount < required)
            return;

        var breachedThreshold = ThresholdEvaluator.NearestBreachedThreshold(
            smoothedValue,
            threshold.MinValue,
            threshold.MaxValue);
        var breachDirection = ThresholdEvaluator.GetBreachDirection(
            smoothedValue,
            threshold.MinValue,
            threshold.MaxValue);

        var alertEvent = new AlertTriggeredIntegrationEvent(
            sensor.ZoneId,
            sensor.Id,
            sensor.Type,
            smoothedValue,
            breachedThreshold,
            threshold.MinValue,
            threshold.MaxValue,
            breachDirection,
            timestamp);

        await alertRecordRepository.AddAsync(
            new AlertRecord
            {
                ZoneId = sensor.ZoneId,
                SensorId = sensor.Id,
                SensorType = sensor.Type,
                Value = smoothedValue,
                MinThreshold = threshold.MinValue,
                MaxThreshold = threshold.MaxValue,
                BreachedThreshold = breachedThreshold,
                BreachDirection = breachDirection,
                TriggeredAt = timestamp
            },
            cancellationToken);
        await breachTrackerRepository.SaveChangesAsync(cancellationToken);

        alertPublisher.Publish(alertEvent);

        tracker.ConsecutiveCount = 0;
        await breachTrackerRepository.UpsertAsync(tracker, cancellationToken);
        await breachTrackerRepository.SaveChangesAsync(cancellationToken);
    }
}
