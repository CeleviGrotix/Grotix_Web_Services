using GrotixBackend.CultivationArea.Domain.Model.Aggregates;

namespace GrotixBackend.CultivationArea.Application.Internal;

public interface IAnalysisReportService
{
    Task<AnalysisReport> CreateAsync(
        int zoneId,
        string detectedPhase,
        float healthScore,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalysisReport>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default);
}
