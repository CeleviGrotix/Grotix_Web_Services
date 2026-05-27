using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public sealed class MaintenanceService(
    IMicrocontrollerRepository deviceRepository,
    IMaintenanceLogRepository maintenanceLogRepository,
    ITechnicalMaintenanceRepository technicalMaintenanceRepository,
    IHardwareDeviceUnitOfWork unitOfWork) : IMaintenanceService
{
    public async Task<MaintenanceLog> RecordMaintenanceLogAsync(
        int deviceId,
        int userId,
        string action,
        string statusAfter,
        CancellationToken cancellationToken = default)
    {
        if (await deviceRepository.GetByIdAsync(deviceId) == null)
            throw new KeyNotFoundException($"Device {deviceId} not found.");

        var log = new MaintenanceLog(deviceId, userId, action, statusAfter);
        await maintenanceLogRepository.AddAsync(log, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return log;
    }

    public async Task<TechnicalMaintenance> RecordTechnicalMaintenanceAsync(
        int staffId,
        int deviceId,
        string type,
        string description,
        string? results,
        CancellationToken cancellationToken = default)
    {
        if (await deviceRepository.GetByIdAsync(deviceId) == null)
            throw new KeyNotFoundException($"Device {deviceId} not found.");

        var record = new TechnicalMaintenance(staffId, deviceId, type, description, results: results);
        await technicalMaintenanceRepository.AddAsync(record, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return record;
    }

    public Task<IReadOnlyList<MaintenanceLog>> ListMaintenanceLogsAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default) =>
        maintenanceLogRepository.ListByDeviceAsync(deviceId, limit, cancellationToken);

    public Task<IReadOnlyList<TechnicalMaintenance>> ListTechnicalMaintenanceByDeviceAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default) =>
        technicalMaintenanceRepository.ListByDeviceAsync(deviceId, limit, cancellationToken);
}
