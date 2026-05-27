using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.Telemetry.Application.ACL;

public sealed class EffectiveThresholdResolver(
    IZoneQueryService zoneQueryService,
    ICropQueryService cropQueryService,
    IActiveThresholdRepository activeThresholdRepository) : IEffectiveThresholdResolver
{
    public async Task<IReadOnlyList<EffectiveThreshold>> ResolveForZoneAsync(
        int zoneId,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null)
            return Array.Empty<EffectiveThreshold>();

        var crop = await cropQueryService.Handle(new GetCropByIdQuery(zone.CropId));
        if (crop == null)
            return Array.Empty<EffectiveThreshold>();

        var overrides = (await activeThresholdRepository.ListByZoneAsync(zoneId, cancellationToken))
            .ToDictionary(t => SensorTypes.Normalize(t.SensorType), StringComparer.OrdinalIgnoreCase);

        var result = new List<EffectiveThreshold>();
        foreach (var sensorType in SensorTypes.All)
        {
            if (overrides.TryGetValue(sensorType, out var custom) &&
                custom.MinValue.HasValue &&
                custom.MaxValue.HasValue)
            {
                result.Add(new EffectiveThreshold(sensorType, custom.MinValue.Value, custom.MaxValue.Value, "custom"));
                continue;
            }

            var defaults = CropThresholdDefaults.ForSensorType(crop, sensorType);
            if (defaults.HasValue)
                result.Add(new EffectiveThreshold(sensorType, defaults.Value.Min, defaults.Value.Max, "crop"));
        }

        return result;
    }

    public async Task<EffectiveThreshold?> ResolveForSensorAsync(
        int zoneId,
        string sensorType,
        CancellationToken cancellationToken = default)
    {
        var all = await ResolveForZoneAsync(zoneId, cancellationToken);
        var normalized = SensorTypes.Normalize(sensorType);
        return all.FirstOrDefault(t => string.Equals(t.SensorType, normalized, StringComparison.OrdinalIgnoreCase));
    }
}
