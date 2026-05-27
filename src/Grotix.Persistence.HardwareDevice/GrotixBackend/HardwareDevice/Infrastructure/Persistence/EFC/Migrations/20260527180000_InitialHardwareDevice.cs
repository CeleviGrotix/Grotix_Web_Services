using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(HardwareDeviceDbContext))]
[Migration("20260527180000_InitialHardwareDevice")]
public partial class InitialHardwareDevice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "microcontroller",
            columns: table => new
            {
                MicrocontrollerID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", 1),
                ZoneID = table.Column<int>(type: "int", nullable: true),
                Model = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                MacAddress = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                LastSeen = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                BatteryLevel = table.Column<int>(type: "int", nullable: true),
                SignalStrength = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_microcontroller", x => x.MicrocontrollerID));

        migrationBuilder.CreateIndex(
            name: "IX_microcontroller_MacAddress",
            table: "microcontroller",
            column: "MacAddress",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_microcontroller_ZoneID",
            table: "microcontroller",
            column: "ZoneID");

        migrationBuilder.CreateTable(
            name: "sensor",
            columns: table => new
            {
                SensorID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", 1),
                MicrocontrollerID = table.Column<int>(type: "int", nullable: false),
                ZoneID = table.Column<int>(type: "int", nullable: true),
                Type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                Unit = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                Pin = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                MinPhysical = table.Column<double>(type: "double", nullable: true),
                MaxPhysical = table.Column<double>(type: "double", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_sensor", x => x.SensorID));

        migrationBuilder.CreateIndex(
            name: "IX_sensor_MicrocontrollerID",
            table: "sensor",
            column: "MicrocontrollerID");

        migrationBuilder.CreateTable(
            name: "actuator",
            columns: table => new
            {
                ActuatorID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", 1),
                MicrocontrollerID = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                Pin = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                CurrentState = table.Column<bool>(type: "tinyint(1)", nullable: false),
                LastSeen = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_actuator", x => x.ActuatorID));

        migrationBuilder.CreateIndex(
            name: "IX_actuator_MicrocontrollerID",
            table: "actuator",
            column: "MicrocontrollerID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "actuator");
        migrationBuilder.DropTable(name: "sensor");
        migrationBuilder.DropTable(name: "microcontroller");
    }
}
