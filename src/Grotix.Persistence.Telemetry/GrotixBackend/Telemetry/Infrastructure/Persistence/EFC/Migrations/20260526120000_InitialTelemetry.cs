using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(TelemetryDbContext))]
[Migration("20260526120000_InitialTelemetry")]
public partial class InitialTelemetry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "sensor",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false),
                DeviceId = table.Column<int>(type: "integer", nullable: false),
                ZoneId = table.Column<int>(type: "integer", nullable: false),
                Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                MinPhysical = table.Column<double>(type: "double precision", nullable: true),
                MaxPhysical = table.Column<double>(type: "double precision", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_sensor", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_sensor_ZoneId",
            table: "sensor",
            column: "ZoneId");

        migrationBuilder.CreateTable(
            name: "sensor_reading",
            columns: table => new
            {
                SensorId = table.Column<int>(type: "integer", nullable: false),
                Timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                Value = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_sensor_reading", x => new { x.SensorId, x.Timestamp }));

        migrationBuilder.CreateTable(
            name: "active_thresholds",
            columns: table => new
            {
                ZoneId = table.Column<int>(type: "integer", nullable: false),
                SensorType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                MinValue = table.Column<double>(type: "double precision", nullable: true),
                MaxValue = table.Column<double>(type: "double precision", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_active_thresholds", x => new { x.ZoneId, x.SensorType }));

        migrationBuilder.CreateTable(
            name: "threshold_breach_tracker",
            columns: table => new
            {
                ZoneId = table.Column<int>(type: "integer", nullable: false),
                SensorId = table.Column<int>(type: "integer", nullable: false),
                SensorType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ConsecutiveCount = table.Column<int>(type: "integer", nullable: false),
                LastEvaluatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_threshold_breach_tracker", x => new { x.ZoneId, x.SensorId }));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "threshold_breach_tracker");
        migrationBuilder.DropTable(name: "active_thresholds");
        migrationBuilder.DropTable(name: "sensor_reading");
        migrationBuilder.DropTable(name: "sensor");
    }
}
