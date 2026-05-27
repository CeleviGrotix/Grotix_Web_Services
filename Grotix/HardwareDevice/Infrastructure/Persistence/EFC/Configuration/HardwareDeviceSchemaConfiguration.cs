using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
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
    }
}
