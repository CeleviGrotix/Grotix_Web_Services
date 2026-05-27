using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.Telemetry.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class IrrigationContextService(
    IZoneQueryService zoneQueryService,
    ICropQueryService cropQueryService,
    HardwareDeviceDbContext hardwareDb,
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

        var humiditySensor = await hardwareDb.Sensors.AsNoTracking()
            .Where(s => s.ZoneId == zoneId)
            .Where(s => s.Type.Contains("HUMID") || s.Type.Contains("MOIST"))
            .OrderBy(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        double? currentHumidity = null;
        if (humiditySensor != null)
            currentHumidity = await sensorReadingRepository.GetLatestSmoothedValueAsync(
                humiditySensor.Id,
                cancellationToken);

        return new IrrigationZoneContext(
            zoneId,
            zone.CropId,
            crop.OptimalHumidity,
            currentHumidity);
    }
}
