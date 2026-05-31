using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class TelemetryQueryService(
    ISensorReadingRepository sensorReadingRepository) : ITelemetryQueryService
{
    public async Task<ZoneTelemetryHistory?> GetZoneHistoryAsync(
        int zoneId,
        DateTime? startTime,
        DateTime? endTime,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var end = endTime?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = startTime?.ToUniversalTime() ?? end.AddDays(-7);
        if (start > end)
            (start, end) = (end, start);

        var readings = await sensorReadingRepository.ListByZoneAsync(
            zoneId, start, end, limit, cancellationToken);

        if (readings.Count == 0)
            return null;

        var points = readings
            .Select(r => new DeviceReadingPoint(
                r.DeviceId,
                r.Timestamp,
                r.Temperature,
                r.HumidityAir,
                r.HumiditySoil,
                r.LightIntensity))
            .ToList();

        return new ZoneTelemetryHistory(zoneId, start, end, points);
    }
}
