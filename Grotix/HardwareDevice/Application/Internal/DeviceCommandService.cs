using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Repositories;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public sealed class DeviceCommandService(
    IMicrocontrollerRepository deviceRepository,
    IDeviceSensorRepository sensorRepository,
    IDeviceActuatorRepository actuatorRepository,
    IHardwareDeviceUnitOfWork unitOfWork,
    IZoneAccessService zoneAccessService,
    ITelemetryCatalogSyncService telemetryCatalogSync) : IDeviceCommandService
{
    public async Task<Microcontroller> RegisterAsync(
        RegisterDeviceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ZoneId.HasValue && !await zoneAccessService.ZoneExistsAsync(request.ZoneId.Value))
            throw new ArgumentException("La zona no existe.");

        if (await deviceRepository.GetByMacAddressAsync(request.MacAddress) != null)
            throw new ArgumentException("MacAddress ya registrada.");

        var device = new Microcontroller(request.Model, request.MacAddress, request.ZoneId);
        await deviceRepository.AddAsync(device);
        await unitOfWork.CompleteAsync(cancellationToken);

        if (request.Sensors != null)
        {
            foreach (var sensorReq in request.Sensors)
            {
                var sensor = new DeviceSensor(
                    device.Id,
                    sensorReq.Type,
                    sensorReq.Unit,
                    sensorReq.Pin,
                    device.ZoneId,
                    sensorReq.MinPhysical,
                    sensorReq.MaxPhysical);
                await sensorRepository.AddAsync(sensor);
                await unitOfWork.CompleteAsync(cancellationToken);
                await telemetryCatalogSync.SyncSensorAsync(sensor, cancellationToken);
            }
        }

        return device;
    }

    public async Task UpdateAsync(
        int deviceId,
        UpdateDeviceRequest request,
        CancellationToken cancellationToken = default)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId)
            ?? throw new KeyNotFoundException($"Dispositivo {deviceId} no encontrado.");

        if (request.ZoneId.HasValue && !await zoneAccessService.ZoneExistsAsync(request.ZoneId.Value))
            throw new ArgumentException("La zona no existe.");

        if (!string.IsNullOrWhiteSpace(request.MacAddress))
        {
            var other = await deviceRepository.GetByMacAddressAsync(request.MacAddress);
            if (other != null && other.Id != deviceId)
                throw new ArgumentException("MacAddress ya registrada.");
        }

        device.Update(request.Model, request.MacAddress, request.ZoneId);
        await unitOfWork.CompleteAsync(cancellationToken);

        if (request.ZoneId.HasValue)
        {
            await sensorRepository.AssignZoneToDeviceAsync(deviceId, request.ZoneId, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            var sensors = await sensorRepository.ListByDeviceAsync(deviceId);
            foreach (var sensor in sensors)
                await telemetryCatalogSync.SyncSensorAsync(sensor, cancellationToken);
        }
    }

    public async Task DeleteAsync(int deviceId, CancellationToken cancellationToken = default)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId)
            ?? throw new KeyNotFoundException($"Dispositivo {deviceId} no encontrado.");

        var sensorCount = await sensorRepository.CountByDeviceAsync(deviceId);
        var actuatorCount = await actuatorRepository.CountByDeviceAsync(deviceId);
        if (sensorCount > 0 || actuatorCount > 0)
            throw new InvalidOperationException("No se puede eliminar un dispositivo con sensores o actuadores asociados.");

        await deviceRepository.DeleteAsync(device);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task LinkToZoneAsync(int deviceId, int zoneId, CancellationToken cancellationToken = default)
    {
        if (!await zoneAccessService.ZoneExistsAsync(zoneId))
            throw new ArgumentException("La zona no existe.");

        var device = await deviceRepository.GetByIdAsync(deviceId)
            ?? throw new KeyNotFoundException($"Dispositivo {deviceId} no encontrado.");

        device.LinkToZone(zoneId);
        await unitOfWork.CompleteAsync(cancellationToken);

        await sensorRepository.AssignZoneToDeviceAsync(deviceId, zoneId, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        var sensors = await sensorRepository.ListByDeviceAsync(deviceId);
        foreach (var sensor in sensors)
            await telemetryCatalogSync.SyncSensorAsync(sensor, cancellationToken);
    }

    public async Task UnlinkFromZoneAsync(int deviceId, int zoneId, CancellationToken cancellationToken = default)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId)
            ?? throw new KeyNotFoundException($"Dispositivo {deviceId} no encontrado.");

        if (device.ZoneId != zoneId)
            throw new ArgumentException("El dispositivo no está vinculado a esa zona.");

        device.UnlinkFromZone();
        await unitOfWork.CompleteAsync(cancellationToken);

        await sensorRepository.AssignZoneToDeviceAsync(deviceId, null, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
