using GrotixBackend.Telemetry.Application.ACL;

namespace GrotixBackend.Telemetry.Application.Internal;

public interface IZoneThresholdService
{
    Task<IReadOnlyList<EffectiveThreshold>> GetEffectiveThresholdsAsync(
        int zoneId,
        CancellationToken cancellationToken = default);

    Task UpdateCustomThresholdsAsync(
        int zoneId,
        IReadOnlyList<ThresholdUpdateRequest> updates,
        CancellationToken cancellationToken = default);
}

public sealed record ThresholdUpdateRequest(string SensorType, double? MinValue, double? MaxValue);
