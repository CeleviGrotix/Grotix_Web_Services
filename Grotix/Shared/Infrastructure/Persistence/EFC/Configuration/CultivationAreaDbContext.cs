using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class CultivationAreaDbContext(DbContextOptions<CultivationAreaDbContext> options) : DbContext(options)
{
    public DbSet<Farm> Farms { get; set; }
    public DbSet<Crop> Crops { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<ZoneMember> ZoneMembers { get; set; }
    public DbSet<AnalysisReport> AnalysisReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureCultivationAreaSchema();
    }
}
