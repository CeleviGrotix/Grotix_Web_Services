using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class IrrigationContextService(
    IZoneQueryService zoneQueryService,
    ICropQueryService cropQueryService,
    ISensorReadingRepository sensorReadingRepository) : IIrrigationContextService
{
    public async Task<IrrigationZoneContext?> GetZoneContextAsync(
        int zoneId,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null)
            return null;

        var crop = await cropQueryService.Handle(new GetCropByIdQuery(zone.CropId));
        if (crop == null)
            return null;

        var latestReadings = await sensorReadingRepository.ListByZoneAsync(
            zoneId, start: null, end: null, limit: 1, cancellationToken);

        double? currentHumiditySoil = latestReadings.Count > 0
            ? latestReadings[0].HumiditySoil
            : null;

        return new IrrigationZoneContext(
            zoneId,
            zone.CropId,
            crop.OptimalHumiditySoil,
            currentHumiditySoil,
            zone.IrrigationMode);
    }
}
