using GrotixBackend.CultivationArea.Application.Internal;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.CultivationArea.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixCultivationAreaModule(this IServiceCollection services) =>
        AddGrotixCultivationAreaModule(services, configuration: null);

    public static IServiceCollection AddGrotixCultivationAreaModule(
        this IServiceCollection services,
        IConfiguration? configuration)
    {
        services.AddScoped<IFarmCommandService, FarmCommandService>();
        services.AddScoped<IFarmQueryService, FarmQueryService>();
        services.AddScoped<IZoneCommandService, ZoneCommandService>();
        services.AddScoped<IZoneQueryService, ZoneQueryService>();
        services.AddScoped<ICropCommandService, CropCommandService>();
        services.AddScoped<ICropQueryService, CropQueryService>();
        services.AddScoped<IAnalysisReportService, AnalysisReportService>();

        if (configuration != null)
            services.AddGrotixZoneReportServices(configuration);

        return services;
    }
}
