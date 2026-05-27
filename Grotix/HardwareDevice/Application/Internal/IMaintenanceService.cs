using GrotixBackend.HardwareDevice.Domain.Model.Entities;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IMaintenanceService
{
    Task<MaintenanceLog> RecordMaintenanceLogAsync(
        int deviceId,
        int userId,
        string action,
        string statusAfter,
        CancellationToken cancellationToken = default);

    Task<TechnicalMaintenance> RecordTechnicalMaintenanceAsync(
        int staffId,
        int deviceId,
        string type,
        string description,
        string? results,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MaintenanceLog>> ListMaintenanceLogsAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TechnicalMaintenance>> ListTechnicalMaintenanceByDeviceAsync(
        int deviceId,
        int limit,
        CancellationToken cancellationToken = default);
}
