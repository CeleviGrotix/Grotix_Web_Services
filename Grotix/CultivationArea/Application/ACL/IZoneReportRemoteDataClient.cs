using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;

namespace GrotixBackend.CultivationArea.Application.ACL;

public interface IZoneReportRemoteDataClient
{
    Task<string?> GetAssociationNameAsync(int associationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ZoneReportDeviceInfo>> GetZoneDevicesAsync(
        int zoneId,
        CancellationToken cancellationToken = default);

    Task<ZoneReportTelemetrySummary> GetTelemetrySummaryAsync(
        int zoneId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<ZoneReportIrrigationSummary> GetIrrigationSummaryAsync(
        int zoneId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ZoneReportAlertInfo>> GetAlertsAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default);
}
