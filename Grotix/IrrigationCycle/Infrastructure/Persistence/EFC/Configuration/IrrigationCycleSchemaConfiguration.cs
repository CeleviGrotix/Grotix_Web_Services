using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;

public static class IrrigationCycleSchemaConfiguration
{
    public static void ConfigureIrrigationCycleSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IrrigationCycleRecord>(e =>
        {
            e.ToTable("irrigation_cycle");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("CycleID").ValueGeneratedOnAdd();
            e.Property(c => c.ZoneId).HasColumnName("ZoneID");
            e.Property(c => c.StartTime);
            e.Property(c => c.EndTime);
            e.Property(c => c.VolumeLiters);
            e.Property(c => c.DurationMinutes);
            e.Property(c => c.Status).HasMaxLength(32).IsRequired();
            e.Property(c => c.AbortReason).HasMaxLength(256);
            e.HasIndex(c => c.ZoneId);
            e.HasIndex(c => new { c.ZoneId, c.Status });
        });

        modelBuilder.Entity<IrrigationSchedule>(e =>
        {
            e.ToTable("irrigation_schedule");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("ScheduleID").ValueGeneratedOnAdd();
            e.Property(s => s.ZoneId).HasColumnName("ZoneID");
            e.Property(s => s.DaysOfTheWeek).HasMaxLength(64).IsRequired();
            e.Property(s => s.StartTime).HasColumnType("time");
            e.Property(s => s.DurationMinutes);
            e.Property(s => s.IsActive).HasColumnName("isActive");
            e.Property(s => s.CreatedAt);
            e.HasIndex(s => s.ZoneId);
        });
    }
}
