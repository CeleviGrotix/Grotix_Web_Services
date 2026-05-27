using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Application.ACL;

/// <summary>Replica sensores del inventario Core hacia Timescale para ingesta de telemetría.</summary>
public interface ITelemetryCatalogSyncService
{
    Task SyncSensorAsync(DeviceSensor sensor, CancellationToken cancellationToken = default);
}
