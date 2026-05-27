using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(IrrigationCycleDbContext))]
partial class IrrigationCycleDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

        modelBuilder.Entity("GrotixBackend.IrrigationCycle.Domain.Model.Aggregates.IrrigationCycleRecord", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").HasColumnName("CycleID");
            b.Property<int>("ZoneId").HasColumnType("int").HasColumnName("ZoneID");
            b.Property<DateTime>("StartTime").HasColumnType("datetime(6)");
            b.Property<DateTime?>("EndTime").HasColumnType("datetime(6)");
            b.Property<double>("VolumeLiters").HasColumnType("double");
            b.Property<int>("DurationMinutes").HasColumnType("int");
            b.Property<string>("Status").IsRequired().HasMaxLength(32).HasColumnType("varchar(32)");
            b.Property<string>("AbortReason").HasMaxLength(256).HasColumnType("varchar(256)");
            b.HasKey("Id");
            b.HasIndex("ZoneId");
            b.HasIndex("ZoneId", "Status");
            b.ToTable("irrigation_cycle");
        });

        modelBuilder.Entity("GrotixBackend.IrrigationCycle.Domain.Model.Aggregates.IrrigationSchedule", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").HasColumnName("ScheduleID");
            b.Property<int>("ZoneId").HasColumnType("int").HasColumnName("ZoneID");
            b.Property<string>("DaysOfTheWeek").IsRequired().HasMaxLength(64).HasColumnType("varchar(64)");
            b.Property<TimeOnly>("StartTime").HasColumnType("time");
            b.Property<int>("DurationMinutes").HasColumnType("int");
            b.Property<bool>("IsActive").HasColumnType("tinyint(1)").HasColumnName("isActive");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
            b.HasKey("Id");
            b.HasIndex("ZoneId");
            b.ToTable("irrigation_schedule");
        });
    }
}
