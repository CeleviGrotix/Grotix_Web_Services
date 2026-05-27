using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.ACL;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Services;

namespace GrotixBackend.Telemetry.Application.Internal.Alerting;

public sealed class AlertEvaluationService(
    IEffectiveThresholdResolver thresholdResolver,
    IThresholdBreachTrackerRepository breachTrackerRepository,
    IAlertPublisher alertPublisher) : IAlertEvaluationService
{
    private const int SustainedCyclesBeforeAlert = 4;

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

        if (tracker.ConsecutiveCount < SustainedCyclesBeforeAlert)
            return;

        var breachedThreshold = ThresholdEvaluator.NearestBreachedThreshold(
            smoothedValue,
            threshold.MinValue,
            threshold.MaxValue);

        alertPublisher.Publish(new AlertTriggeredIntegrationEvent(
            sensor.ZoneId,
            sensor.Id,
            smoothedValue,
            breachedThreshold,
            timestamp));

        tracker.ConsecutiveCount = 0;
        await breachTrackerRepository.UpsertAsync(tracker, cancellationToken);
        await breachTrackerRepository.SaveChangesAsync(cancellationToken);
    }
}
