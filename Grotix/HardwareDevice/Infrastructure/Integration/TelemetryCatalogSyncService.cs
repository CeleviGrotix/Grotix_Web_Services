using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class TelemetryCatalogSyncService(
    TelemetryDbContext telemetryDb,
    ILogger<TelemetryCatalogSyncService> logger) : ITelemetryCatalogSyncService
{
    public async Task SyncSensorAsync(DeviceSensor sensor, CancellationToken cancellationToken = default)
    {
        if (!sensor.ZoneId.HasValue)
            return;

        try
        {
            var existing = await telemetryDb.Sensors.FindAsync([sensor.Id], cancellationToken);
            if (existing == null)
            {
                telemetryDb.Sensors.Add(new Sensor
                {
                    Id = sensor.Id,
                    DeviceId = sensor.MicrocontrollerId,
                    ZoneId = sensor.ZoneId.Value,
                    Type = sensor.Type,
                    Unit = sensor.Unit,
                    MinPhysical = sensor.MinPhysical,
                    MaxPhysical = sensor.MaxPhysical
                });
            }
            else
            {
                existing.DeviceId = sensor.MicrocontrollerId;
                existing.ZoneId = sensor.ZoneId.Value;
                existing.Type = sensor.Type;
                existing.Unit = sensor.Unit;
                existing.MinPhysical = sensor.MinPhysical;
                existing.MaxPhysical = sensor.MaxPhysical;
            }

            await telemetryDb.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo sincronizar sensor {SensorId} en Timescale.", sensor.Id);
        }
    }
}
