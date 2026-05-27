using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Integration;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.HardwareDevice.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixHardwareDevicePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var mysqlServerVersion = ResolveMySqlServerVersion(configuration["MySql:ServerVersion"]);

        services.AddDbContext<HardwareDeviceDbContext>(options =>
            options.UseMySql(
                connectionString,
                mysqlServerVersion,
                mysql =>
                {
                    mysql.MigrationsHistoryTable("__EFMigrationsHistory_HardwareDevice");
                    mysql.MigrationsAssembly("Grotix.Persistence.HardwareDevice");
                }));

        services.AddScoped<IHardwareDeviceUnitOfWork, HardwareDeviceUnitOfWork>();
        services.AddScoped<IMicrocontrollerRepository, MicrocontrollerRepository>();
        services.AddScoped<IDeviceSensorRepository, DeviceSensorRepository>();
        services.AddScoped<IDeviceActuatorRepository, DeviceActuatorRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
        services.AddScoped<ITechnicalMaintenanceRepository, TechnicalMaintenanceRepository>();
        services.AddScoped<IActionQueueRepository, ActionQueueRepository>();
        services.AddScoped<IDeviceStatusChangedPublisher, RabbitMqDeviceStatusChangedPublisher>();
        services.AddScoped<ITelemetryCatalogSyncService, TelemetryCatalogSyncService>();
        services.AddHostedService<HardwareDatabaseInitializer>();

        return services;
    }

    private static MySqlServerVersion ResolveMySqlServerVersion(string? mysqlVersionString)
    {
        if (string.IsNullOrWhiteSpace(mysqlVersionString))
            return new MySqlServerVersion(new Version(8, 0, 36));

        var segments = mysqlVersionString.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
        var major = segments.Length > 0 ? int.Parse(segments[0]) : 8;
        var minor = segments.Length > 1 ? int.Parse(segments[1]) : 0;
        var build = segments.Length > 2 ? int.Parse(segments[2]) : 0;
        return new MySqlServerVersion(new Version(major, minor, build));
    }
}
