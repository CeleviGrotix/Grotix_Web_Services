using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;

public class HardwareDeviceDbContext(DbContextOptions<HardwareDeviceDbContext> options) : DbContext(options)
{
    public DbSet<Microcontroller> Microcontrollers => Set<Microcontroller>();
    public DbSet<DeviceSensor> Sensors => Set<DeviceSensor>();
    public DbSet<DeviceActuator> Actuators => Set<DeviceActuator>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();
    public DbSet<TechnicalMaintenance> TechnicalMaintenances => Set<TechnicalMaintenance>();
    public DbSet<ActionQueueItem> ActionQueue => Set<ActionQueueItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ConfigureHardwareDeviceSchema();
}
