using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Repositories;
using GrotixBackend.IrrigationCycle.Infrastructure.Integration;
using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.IrrigationCycle.Infrastructure.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixIrrigationCyclePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var mysqlServerVersion = ResolveMySqlServerVersion(configuration["MySql:ServerVersion"]);

        services.AddDbContext<IrrigationCycleDbContext>(options =>
            options.UseMySql(
                connectionString,
                mysqlServerVersion,
                mysql =>
                {
                    mysql.MigrationsHistoryTable("__EFMigrationsHistory_IrrigationCycle");
                    mysql.MigrationsAssembly("Grotix.Persistence.IrrigationCycle");
                }));

        services.AddDbContext<HardwareDeviceDbContext>(options =>
            options.UseMySql(
                connectionString,
                mysqlServerVersion,
                mysql =>
                {
                    mysql.MigrationsHistoryTable("__EFMigrationsHistory_HardwareDevice");
                    mysql.MigrationsAssembly("Grotix.Persistence.HardwareDevice");
                }));

        services.AddGrotixTelemetryPersistence(configuration);

        services.AddScoped<IIrrigationUnitOfWork, IrrigationUnitOfWork>();
        services.AddScoped<IIrrigationCycleRepository, IrrigationCycleRepository>();
        services.AddScoped<IIrrigationScheduleRepository, IrrigationScheduleRepository>();
        services.AddScoped<IIrrigationContextService, IrrigationContextService>();
        services.AddScoped<IActuatorControlService, ActuatorControlService>();
        services.AddScoped<IIrrigationCompletedPublisher, RabbitMqIrrigationCompletedPublisher>();
        services.AddHostedService<IrrigationDatabaseInitializer>();
        services.AddHostedService<IrrigationCycleCompletionHostedService>();

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
