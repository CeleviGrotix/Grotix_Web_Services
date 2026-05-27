using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(TelemetryDbContext))]
partial class TelemetryDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

        modelBuilder.Entity("GrotixBackend.Telemetry.Domain.Model.Entities.ActiveThreshold", b =>
        {
            b.Property<int>("ZoneId").HasColumnType("integer").HasColumnName("zone_id");
            b.Property<string>("SensorType").HasMaxLength(64).HasColumnType("character varying(64)").HasColumnName("sensor_type");
            b.Property<double?>("MinValue").HasColumnType("double precision").HasColumnName("min_value");
            b.Property<double?>("MaxValue").HasColumnType("double precision").HasColumnName("max_value");
            b.HasKey("ZoneId", "SensorType");
            b.ToTable("active_thresholds");
        });

        modelBuilder.Entity("GrotixBackend.Telemetry.Domain.Model.Entities.Sensor", b =>
        {
            b.Property<int>("Id").HasColumnType("integer").HasColumnName("id");
            b.Property<int>("DeviceId").HasColumnType("integer").HasColumnName("device_id");
            b.Property<int>("ZoneId").HasColumnType("integer").HasColumnName("zone_id");
            b.Property<string>("Type").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)").HasColumnName("type");
            b.Property<string>("Unit").IsRequired().HasMaxLength(16).HasColumnType("character varying(16)").HasColumnName("unit");
            b.Property<double?>("MinPhysical").HasColumnType("double precision").HasColumnName("min_physical");
            b.Property<double?>("MaxPhysical").HasColumnType("double precision").HasColumnName("max_physical");
            b.HasKey("Id");
            b.HasIndex("ZoneId");
            b.ToTable("sensor");
        });

        modelBuilder.Entity("GrotixBackend.Telemetry.Domain.Model.Entities.SensorReading", b =>
        {
            b.Property<int>("SensorId").HasColumnType("integer").HasColumnName("sensor_id");
            b.Property<DateTime>("Timestamp").HasColumnType("timestamptz").HasColumnName("timestamp");
            b.Property<double>("Value").HasColumnType("double precision").HasColumnName("value");
            b.HasKey("SensorId", "Timestamp");
            b.ToTable("sensor_reading");
        });

        modelBuilder.Entity("GrotixBackend.Telemetry.Domain.Model.Entities.ThresholdBreachTracker", b =>
        {
            b.Property<int>("ZoneId").HasColumnType("integer").HasColumnName("zone_id");
            b.Property<int>("SensorId").HasColumnType("integer").HasColumnName("sensor_id");
            b.Property<string>("SensorType").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)").HasColumnName("sensor_type");
            b.Property<int>("ConsecutiveCount").HasColumnType("integer").HasColumnName("consecutive_count");
            b.Property<DateTime?>("LastEvaluatedAt").HasColumnType("timestamptz").HasColumnName("last_evaluated_at");
            b.HasKey("ZoneId", "SensorId");
            b.ToTable("threshold_breach_tracker");
        });
    }
}
