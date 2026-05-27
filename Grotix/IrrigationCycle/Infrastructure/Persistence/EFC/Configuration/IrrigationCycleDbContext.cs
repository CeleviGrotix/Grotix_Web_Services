using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;

public class IrrigationCycleDbContext(DbContextOptions<IrrigationCycleDbContext> options) : DbContext(options)
{
    public DbSet<IrrigationCycleRecord> Cycles => Set<IrrigationCycleRecord>();
    public DbSet<IrrigationSchedule> Schedules => Set<IrrigationSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ConfigureIrrigationCycleSchema();
}
