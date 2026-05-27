using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal;

public sealed class AnalysisReportService(
    IZoneRepository zoneRepository,
    IAnalysisReportRepository analysisReportRepository,
    ICultivationAreaUnitOfWork unitOfWork) : IAnalysisReportService
{
    public async Task<AnalysisReport> CreateAsync(
        int zoneId,
        string detectedPhase,
        float healthScore,
        CancellationToken cancellationToken = default)
    {
        if (await zoneRepository.GetByIdAsync(zoneId) == null)
            throw new KeyNotFoundException($"Zone {zoneId} not found.");

        var report = new AnalysisReport(zoneId, detectedPhase, healthScore);
        await analysisReportRepository.AddAsync(report, cancellationToken);
        await unitOfWork.CompleteAsync();
        return report;
    }

    public Task<IReadOnlyList<AnalysisReport>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default) =>
        analysisReportRepository.ListByZoneAsync(zoneId, limit, cancellationToken);
}
