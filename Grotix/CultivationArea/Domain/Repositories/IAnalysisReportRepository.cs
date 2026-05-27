using GrotixBackend.CultivationArea.Domain.Model.Aggregates;

namespace GrotixBackend.CultivationArea.Domain.Repositories;

public interface IAnalysisReportRepository
{
    Task AddAsync(AnalysisReport report, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalysisReport>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default);

    Task<AnalysisReport?> GetByIdAsync(int reportId, CancellationToken cancellationToken = default);
}
