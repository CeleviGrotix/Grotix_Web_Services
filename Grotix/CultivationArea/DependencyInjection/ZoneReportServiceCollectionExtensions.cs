using GrotixBackend.CultivationArea.Application.ACL;
using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;
using GrotixBackend.CultivationArea.Infrastructure.Http;
using GrotixBackend.CultivationArea.Infrastructure.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.CultivationArea.DependencyInjection;

public static class ZoneReportServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixZoneReportServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ZoneReportOptions>(configuration.GetSection(ZoneReportOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddHttpClient<IZoneReportRemoteDataClient, ZoneReportRemoteDataClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IZoneReportService, ZoneReportService>();
        services.AddSingleton<IZoneReportPdfRenderer, QuestPdfZoneReportRenderer>();

        return services;
    }
}
