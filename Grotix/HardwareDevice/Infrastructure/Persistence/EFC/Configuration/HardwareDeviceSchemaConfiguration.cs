using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;

public static class HardwareDeviceSchemaConfiguration
{
    public static void ConfigureHardwareDeviceSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Microcontroller>(e =>
        {
            e.ToTable("microcontroller");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("MicrocontrollerID").ValueGeneratedOnAdd();
            e.Property(d => d.ZoneId).HasColumnName("ZoneID");
            e.Property(d => d.Model).HasMaxLength(120).IsRequired();
            e.Property(d => d.MacAddress).HasMaxLength(32).IsRequired();
            e.HasIndex(d => d.MacAddress).IsUnique();
            e.Property(d => d.Status).HasMaxLength(32).IsRequired();
            e.Property(d => d.LastSeen);
            e.Property(d => d.BatteryLevel);
            e.Property(d => d.SignalStrength);
            e.Property(d => d.CreatedAt);
            e.HasIndex(d => d.ZoneId);
        });

        modelBuilder.Entity<DeviceSensor>(e =>
        {
            e.ToTable("sensor");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("SensorID").ValueGeneratedOnAdd();
            e.Property(s => s.MicrocontrollerId).HasColumnName("MicrocontrollerID");
            e.Property(s => s.ZoneId).HasColumnName("ZoneID");
            e.Property(s => s.Type).HasMaxLength(64).IsRequired();
            e.Property(s => s.Unit).HasMaxLength(16).IsRequired();
            e.Property(s => s.Pin);
            e.Property(s => s.Status).HasMaxLength(32).IsRequired();
            e.Property(s => s.LastSeen);
            e.Property(s => s.MinPhysical);
            e.Property(s => s.MaxPhysical);
            e.HasIndex(s => s.MicrocontrollerId);
        });

        modelBuilder.Entity<DeviceActuator>(e =>
        {
            e.ToTable("actuator");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("ActuatorID").ValueGeneratedOnAdd();
            e.Property(a => a.MicrocontrollerId).HasColumnName("MicrocontrollerID");
            e.Property(a => a.Type).HasMaxLength(64).IsRequired();
            e.Property(a => a.Pin);
            e.Property(a => a.Status).HasMaxLength(32).IsRequired();
            e.Property(a => a.CurrentState);
            e.Property(a => a.LastSeen);
            e.HasIndex(a => a.MicrocontrollerId);
        });

        modelBuilder.Entity<MaintenanceLog>(e =>
        {
            e.ToTable("maintenance_log");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasColumnName("LogID").ValueGeneratedOnAdd();
            e.Property(m => m.DeviceId).HasColumnName("DeviceID");
            e.Property(m => m.UserId).HasColumnName("UserID");
            e.Property(m => m.Action).HasMaxLength(256).IsRequired();
            e.Property(m => m.StatusAfter).HasMaxLength(32).IsRequired();
            e.Property(m => m.Timestamp);
            e.HasIndex(m => m.DeviceId);
            e.HasIndex(m => m.UserId);
        });

        modelBuilder.Entity<TechnicalMaintenance>(e =>
        {
            e.ToTable("technical_maintenance");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasColumnName("MaintenanceID").ValueGeneratedOnAdd();
            e.Property(m => m.StaffId).HasColumnName("StaffID");
            e.Property(m => m.DeviceId).HasColumnName("DeviceID");
            e.Property(m => m.Type).HasMaxLength(64).IsRequired();
            e.Property(m => m.Description).HasMaxLength(2000).IsRequired();
            e.Property(m => m.Date);
            e.Property(m => m.Results).HasMaxLength(2000);
            e.HasIndex(m => m.StaffId);
            e.HasIndex(m => m.DeviceId);
        });

        modelBuilder.Entity<ActionQueueItem>(e =>
        {
            e.ToTable("action_queue");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("ActionID").ValueGeneratedOnAdd();
            e.Property(a => a.ActuatorId).HasColumnName("ActuatorID");
            e.Property(a => a.Command).HasMaxLength(32).IsRequired();
            e.Property(a => a.Status).HasMaxLength(32).IsRequired();
            e.Property(a => a.CreatedAt);
            e.HasIndex(a => new { a.ActuatorId, a.Status });
        });
    }
}
