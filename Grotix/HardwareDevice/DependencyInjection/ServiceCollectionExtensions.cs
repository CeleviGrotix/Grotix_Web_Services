using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Application.Internal.Presence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.HardwareDevice.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixHardwareDeviceModule(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        if (configuration is not null)
            services.Configure<DevicePresenceOptions>(configuration.GetSection(DevicePresenceOptions.SectionName));

        services.AddScoped<IZoneAccessService, ZoneAccessService>();
        services.AddScoped<IStaffExistenceService, StaffExistenceService>();
        services.AddScoped<IDevicePresenceService, DevicePresenceService>();
        services.AddScoped<IDeviceQueryService, DeviceQueryService>();
        services.AddScoped<IDeviceCommandService, DeviceCommandService>();
        services.AddScoped<IDeviceDiagnosticService, DeviceDiagnosticService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        return services;
    }
}
