using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public sealed class AnalysisReportRepository(CultivationAreaDbContext context) : IAnalysisReportRepository
{
    public async Task AddAsync(AnalysisReport report, CancellationToken cancellationToken = default) =>
        await context.Set<AnalysisReport>().AddAsync(report, cancellationToken);

    public async Task<IReadOnlyList<AnalysisReport>> ListByZoneAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default) =>
        await context.Set<AnalysisReport>()
            .AsNoTracking()
            .Where(r => r.ZoneId == zoneId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public Task<AnalysisReport?> GetByIdAsync(int reportId, CancellationToken cancellationToken = default) =>
        context.Set<AnalysisReport>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
}
