using GrotixBackend.Telemetry.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;

public class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<ActiveThreshold> ActiveThresholds => Set<ActiveThreshold>();
    public DbSet<ThresholdBreachTracker> ThresholdBreachTrackers => Set<ThresholdBreachTracker>();
    public DbSet<AlertRecord> AlertRecords => Set<AlertRecord>();
    public DbSet<ActuatorLogEntry> ActuatorLogs => Set<ActuatorLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("sensor");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.ZoneId).HasColumnName("zone_id");
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(64).IsRequired();
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(16).IsRequired();
            entity.Property(e => e.MinPhysical).HasColumnName("min_physical");
            entity.Property(e => e.MaxPhysical).HasColumnName("max_physical");
            entity.HasIndex(e => e.ZoneId);
        });

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("sensor_reading");
            entity.HasKey(e => new { e.SensorId, e.Timestamp });
            entity.Property(e => e.SensorId).HasColumnName("sensor_id");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").HasColumnType("timestamptz");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<ActiveThreshold>(entity =>
        {
            entity.ToTable("active_thresholds");
            entity.HasKey(e => new { e.ZoneId, e.SensorType });
            entity.Property(e => e.ZoneId).HasColumnName("zone_id");
            entity.Property(e => e.SensorType).HasColumnName("sensor_type").HasMaxLength(64);
            entity.Property(e => e.MinValue).HasColumnName("min_value");
            entity.Property(e => e.MaxValue).HasColumnName("max_value");
        });

        modelBuilder.Entity<ThresholdBreachTracker>(entity =>
        {
            entity.ToTable("threshold_breach_tracker");
            entity.HasKey(e => new { e.ZoneId, e.SensorId });
            entity.Property(e => e.ZoneId).HasColumnName("zone_id");
            entity.Property(e => e.SensorId).HasColumnName("sensor_id");
            entity.Property(e => e.SensorType).HasColumnName("sensor_type").HasMaxLength(64);
            entity.Property(e => e.ConsecutiveCount).HasColumnName("consecutive_count");
            entity.Property(e => e.LastEvaluatedAt).HasColumnName("last_evaluated_at").HasColumnType("timestamptz");
        });

        modelBuilder.Entity<AlertRecord>(entity =>
        {
            entity.ToTable("alert_log");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ZoneId).HasColumnName("zone_id");
            entity.Property(e => e.SensorId).HasColumnName("sensor_id");
            entity.Property(e => e.SensorType).HasColumnName("sensor_type").HasMaxLength(64);
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.MinThreshold).HasColumnName("min_threshold");
            entity.Property(e => e.MaxThreshold).HasColumnName("max_threshold");
            entity.Property(e => e.BreachedThreshold).HasColumnName("breached_threshold");
            entity.Property(e => e.BreachDirection).HasColumnName("breach_direction").HasMaxLength(16);
            entity.Property(e => e.TriggeredAt).HasColumnName("triggered_at").HasColumnType("timestamptz");
            entity.HasIndex(e => new { e.ZoneId, e.TriggeredAt });
        });

        modelBuilder.Entity<ActuatorLogEntry>(entity =>
        {
            entity.ToTable("actuator_log");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActuatorId).HasColumnName("actuator_id");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(32).IsRequired();
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").HasColumnType("timestamptz");
            entity.Property(e => e.FlowRate).HasColumnName("flow_rate");
            entity.HasIndex(e => new { e.ActuatorId, e.Timestamp });
        });
    }
}
