using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;

public class HardwareDeviceDbContext(DbContextOptions<HardwareDeviceDbContext> options) : DbContext(options)
{
    public DbSet<Microcontroller> Microcontrollers => Set<Microcontroller>();
    public DbSet<DeviceSensor> Sensors => Set<DeviceSensor>();
    public DbSet<DeviceActuator> Actuators => Set<DeviceActuator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ConfigureHardwareDeviceSchema();
}
