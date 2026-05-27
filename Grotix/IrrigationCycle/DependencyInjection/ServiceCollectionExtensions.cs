using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Application.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.IrrigationCycle.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixIrrigationCycleModule(this IServiceCollection services)
    {
        services.AddScoped<IZoneAccessService, ZoneAccessService>();
        services.AddScoped<IIrrigationCommandService, IrrigationCommandService>();
        services.AddScoped<IIrrigationQueryService, IrrigationQueryService>();
        services.AddScoped<IIrrigationScheduleService, IrrigationScheduleService>();
        return services;
    }
}
