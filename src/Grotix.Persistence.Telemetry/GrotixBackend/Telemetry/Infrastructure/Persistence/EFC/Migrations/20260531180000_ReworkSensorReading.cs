using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(TelemetryDbContext))]
[Migration("20260531180000_ReworkSensorReading")]
public partial class ReworkSensorReading : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "sensor_reading");

        migrationBuilder.CreateTable(
            name: "sensor_reading",
            columns: table => new
            {
                device_id  = table.Column<int>(type: "integer", nullable: false),
                timestamp  = table.Column<DateTime>(type: "timestamptz", nullable: false),
                zone_id         = table.Column<int>(type: "integer", nullable: false),
                temperature     = table.Column<double>(type: "double precision", nullable: false),
                humidity_air    = table.Column<double>(type: "double precision", nullable: false),
                humidity_soil   = table.Column<double>(type: "double precision", nullable: false),
                light_intensity = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_sensor_reading", x => new { x.device_id, x.timestamp }));

        migrationBuilder.CreateIndex(
            name: "IX_sensor_reading_zone_id_timestamp",
            table: "sensor_reading",
            columns: new[] { "zone_id", "timestamp" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "sensor_reading");

        migrationBuilder.CreateTable(
            name: "sensor_reading",
            columns: table => new
            {
                sensor_id = table.Column<int>(type: "integer", nullable: false),
                timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                value     = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_sensor_reading", x => new { x.sensor_id, x.timestamp }));
    }
}
