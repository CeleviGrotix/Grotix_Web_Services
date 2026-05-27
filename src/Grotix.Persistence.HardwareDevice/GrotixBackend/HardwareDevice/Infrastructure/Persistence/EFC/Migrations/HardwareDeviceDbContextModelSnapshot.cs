using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(HardwareDeviceDbContext))]
partial class HardwareDeviceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

        modelBuilder.Entity("GrotixBackend.HardwareDevice.Domain.Model.Aggregates.DeviceActuator", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").HasColumnName("ActuatorID");
            b.Property<int>("MicrocontrollerId").HasColumnType("int").HasColumnName("MicrocontrollerID");
            b.Property<string>("Type").IsRequired().HasMaxLength(64).HasColumnType("varchar(64)");
            b.Property<int>("Pin").HasColumnType("int");
            b.Property<string>("Status").IsRequired().HasMaxLength(32).HasColumnType("varchar(32)");
            b.Property<bool>("CurrentState").HasColumnType("tinyint(1)");
            b.Property<DateTime?>("LastSeen").HasColumnType("datetime(6)");
            b.HasKey("Id");
            b.HasIndex("MicrocontrollerId");
            b.ToTable("actuator");
        });

        modelBuilder.Entity("GrotixBackend.HardwareDevice.Domain.Model.Aggregates.DeviceSensor", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").HasColumnName("SensorID");
            b.Property<int>("MicrocontrollerId").HasColumnType("int").HasColumnName("MicrocontrollerID");
            b.Property<int?>("ZoneId").HasColumnType("int").HasColumnName("ZoneID");
            b.Property<string>("Type").IsRequired().HasMaxLength(64).HasColumnType("varchar(64)");
            b.Property<string>("Unit").IsRequired().HasMaxLength(16).HasColumnType("varchar(16)");
            b.Property<int>("Pin").HasColumnType("int");
            b.Property<string>("Status").IsRequired().HasMaxLength(32).HasColumnType("varchar(32)");
            b.Property<double?>("MinPhysical").HasColumnType("double");
            b.Property<double?>("MaxPhysical").HasColumnType("double");
            b.HasKey("Id");
            b.HasIndex("MicrocontrollerId");
            b.ToTable("sensor");
        });

        modelBuilder.Entity("GrotixBackend.HardwareDevice.Domain.Model.Aggregates.Microcontroller", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").HasColumnName("MicrocontrollerID");
            b.Property<int?>("ZoneId").HasColumnType("int").HasColumnName("ZoneID");
            b.Property<string>("Model").IsRequired().HasMaxLength(120).HasColumnType("varchar(120)");
            b.Property<string>("MacAddress").IsRequired().HasMaxLength(32).HasColumnType("varchar(32)");
            b.Property<string>("Status").IsRequired().HasMaxLength(32).HasColumnType("varchar(32)");
            b.Property<DateTime?>("LastSeen").HasColumnType("datetime(6)");
            b.Property<int?>("BatteryLevel").HasColumnType("int");
            b.Property<int?>("SignalStrength").HasColumnType("int");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
            b.HasKey("Id");
            b.HasIndex("MacAddress").IsUnique();
            b.HasIndex("ZoneId");
            b.ToTable("microcontroller");
        });
    }
}
