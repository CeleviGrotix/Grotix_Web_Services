using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Application.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.HardwareDevice.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixHardwareDeviceModule(this IServiceCollection services)
    {
        services.AddScoped<IZoneAccessService, ZoneAccessService>();
        services.AddScoped<IStaffExistenceService, StaffExistenceService>();
        services.AddScoped<IDeviceQueryService, DeviceQueryService>();
        services.AddScoped<IDeviceCommandService, DeviceCommandService>();
        services.AddScoped<IDeviceDiagnosticService, DeviceDiagnosticService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        return services;
    }
}
