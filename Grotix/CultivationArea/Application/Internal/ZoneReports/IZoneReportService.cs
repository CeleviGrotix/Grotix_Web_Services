namespace GrotixBackend.CultivationArea.Application.Internal.ZoneReports;

public interface IZoneReportService
{
    Task<ZoneReportData?> BuildAsync(
        int zoneId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default);

    Task<byte[]> BuildPdfAsync(
        int zoneId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default);
}
