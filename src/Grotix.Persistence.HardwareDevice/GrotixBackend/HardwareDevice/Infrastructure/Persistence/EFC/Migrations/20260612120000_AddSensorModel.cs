using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(HardwareDeviceDbContext))]
[Migration("20260612120000_AddSensorModel")]
public partial class AddSensorModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Model",
            table: "sensor",
            type: "varchar(64)",
            maxLength: 64,
            nullable: false,
            defaultValue: "GENERIC");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Model",
            table: "sensor");
    }
}
