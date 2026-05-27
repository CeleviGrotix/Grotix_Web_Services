namespace GrotixBackend.Telemetry.Application.ACL;

public interface IEffectiveThresholdResolver
{
    Task<IReadOnlyList<EffectiveThreshold>> ResolveForZoneAsync(int zoneId, CancellationToken cancellationToken = default);

    Task<EffectiveThreshold?> ResolveForSensorAsync(
        int zoneId,
        string sensorType,
        CancellationToken cancellationToken = default);
}
