using GrotixBackend.CultivationArea.Application.ACL;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.ZoneReports;

public sealed class ZoneReportService(
    IZoneQueryService zoneQueryService,
    IFarmQueryService farmQueryService,
    ICropQueryService cropQueryService,
    IAnalysisReportRepository analysisReportRepository,
    IZoneReportRemoteDataClient remoteDataClient,
    IZoneReportPdfRenderer pdfRenderer) : IZoneReportService
{
    public async Task<ZoneReportData?> BuildAsync(
        int zoneId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null)
            return null;

        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(zone.FarmId));
        if (farm == null)
            return null;

        var end = (toUtc ?? DateTime.UtcNow).ToUniversalTime();
        var start = (fromUtc ?? end.AddDays(-30)).ToUniversalTime();
        if (start > end)
            (start, end) = (end, start);

        var crop = await cropQueryService.Handle(new GetCropByIdQuery(zone.CropId));
        var associationName = await remoteDataClient.GetAssociationNameAsync(farm.AssociationId, cancellationToken)
            ?? $"Association #{farm.AssociationId}";

        var devicesTask = remoteDataClient.GetZoneDevicesAsync(zoneId, cancellationToken);
        var telemetryTask = remoteDataClient.GetTelemetrySummaryAsync(zoneId, start, end, cancellationToken);
        var irrigationTask = remoteDataClient.GetIrrigationSummaryAsync(zoneId, start, end, cancellationToken);
        var alertsTask = remoteDataClient.GetAlertsAsync(zoneId, 15, cancellationToken);
        var analysisTask = analysisReportRepository.ListByZoneAsync(zoneId, 20, cancellationToken);

        await Task.WhenAll(devicesTask, telemetryTask, irrigationTask, alertsTask, analysisTask);

        var analysisInPeriod = analysisTask.Result
            .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
            .Select(r => new ZoneReportAnalysisInfo(r.DetectedPhase, r.HealthScore, r.CreatedAt))
            .ToList();

        return new ZoneReportData(
            DateTime.UtcNow,
            start,
            end,
            new ZoneReportZoneInfo(
                zone.Id,
                zone.Name,
                crop?.CommonName ?? $"Crop #{zone.CropId}",
                zone.IrrigationMode,
                zone.CurrentPhase,
                zone.Latitude,
                zone.Longitude),
            new ZoneReportFarmInfo(
                farm.Id,
                farm.Name,
                farm.Location,
                farm.AssociationId,
                associationName),
            devicesTask.Result,
            telemetryTask.Result,
            irrigationTask.Result,
            analysisInPeriod,
            alertsTask.Result);
    }

    public async Task<byte[]> BuildPdfAsync(
        int zoneId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var data = await BuildAsync(zoneId, fromUtc, toUtc, cancellationToken);
        if (data == null)
            throw new KeyNotFoundException($"Zone {zoneId} not found.");

        return pdfRenderer.Render(data);
    }
}
