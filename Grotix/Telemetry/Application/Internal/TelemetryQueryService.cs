using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class TelemetryQueryService(
    ISensorRepository sensorRepository,
    ISensorReadingRepository sensorReadingRepository) : ITelemetryQueryService
{
    public async Task<ZoneTelemetryHistory?> GetZoneHistoryAsync(
        int zoneId,
        DateTime? startTime,
        DateTime? endTime,
        IReadOnlyList<string>? sensorTypes,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedTypes = sensorTypes?
            .Select(SensorTypes.Normalize)
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        IReadOnlyList<Domain.Model.Entities.Sensor> sensors;
        if (normalizedTypes is { Count: > 0 })
            sensors = await sensorRepository.ListByZoneAndTypesAsync(zoneId, normalizedTypes, cancellationToken);
        else
            sensors = await sensorRepository.ListByZoneAsync(zoneId, cancellationToken);

        if (sensors.Count == 0)
            return null;

        var end = endTime?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = startTime?.ToUniversalTime() ?? end.AddDays(-7);
        if (start > end)
            (start, end) = (end, start);

        var series = new List<SensorTelemetrySeries>();
        foreach (var sensor in sensors)
        {
            var readings = await sensorReadingRepository.ListBySensorAsync(
                sensor.Id,
                start,
                end,
                limit,
                cancellationToken);

            series.Add(new SensorTelemetrySeries(
                sensor.Id,
                sensor.Type,
                sensor.Unit,
                readings
                    .OrderByDescending(r => r.Timestamp)
                    .Select(r => new SensorReadingPoint(r.Value, r.Timestamp))
                    .ToList()));
        }

        return new ZoneTelemetryHistory(zoneId, start, end, series);
    }
}
