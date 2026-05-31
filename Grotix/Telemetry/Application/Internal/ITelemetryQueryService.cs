namespace GrotixBackend.Telemetry.Application.Internal;

public interface ITelemetryQueryService
{
    Task<ZoneTelemetryHistory?> GetZoneHistoryAsync(
        int zoneId,
        DateTime? startTime,
        DateTime? endTime,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed record ZoneTelemetryHistory(
    int ZoneId,
    DateTime Start,
    DateTime End,
    IReadOnlyList<DeviceReadingPoint> Readings);

public sealed record DeviceReadingPoint(
    int DeviceId,
    DateTime Timestamp,
    double Temperature,
    double HumidityAir,
    double HumiditySoil,
    double LightIntensity);
