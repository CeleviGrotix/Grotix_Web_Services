using GrotixBackend.Telemetry.Application.ACL;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class ZoneThresholdService(
    IEffectiveThresholdResolver thresholdResolver,
    IActiveThresholdRepository activeThresholdRepository) : IZoneThresholdService
{
    public Task<IReadOnlyList<EffectiveThreshold>> GetEffectiveThresholdsAsync(
        int zoneId,
        CancellationToken cancellationToken = default) =>
        thresholdResolver.ResolveForZoneAsync(zoneId, cancellationToken);

    public async Task UpdateCustomThresholdsAsync(
        int zoneId,
        IReadOnlyList<ThresholdUpdateRequest> updates,
        CancellationToken cancellationToken = default)
    {
        foreach (var update in updates)
        {
            var sensorType = SensorTypes.Normalize(update.SensorType);
            if (!SensorTypes.All.Contains(sensorType))
                throw new ArgumentException($"Tipo de sensor no soportado: {update.SensorType}");

            if (!update.MinValue.HasValue && !update.MaxValue.HasValue)
            {
                await activeThresholdRepository.DeleteAsync(zoneId, sensorType, cancellationToken);
                continue;
            }

            if (!update.MinValue.HasValue || !update.MaxValue.HasValue)
                throw new ArgumentException($"MinValue y MaxValue son requeridos para {sensorType} (o ambos null para reset).");

            if (update.MinValue.Value >= update.MaxValue.Value)
                throw new ArgumentException($"MinValue debe ser menor que MaxValue para {sensorType}.");

            await activeThresholdRepository.UpsertAsync(new ActiveThreshold
            {
                ZoneId = zoneId,
                SensorType = sensorType,
                MinValue = update.MinValue,
                MaxValue = update.MaxValue
            }, cancellationToken);
        }

        await activeThresholdRepository.SaveChangesAsync(cancellationToken);
    }
}
