using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(TelemetryDbContext))]
[Migration("20260527140000_TelemetrySnakeCaseColumns")]
public partial class TelemetrySnakeCaseColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(name: "Id", table: "sensor", newName: "id");
        migrationBuilder.RenameColumn(name: "DeviceId", table: "sensor", newName: "device_id");
        migrationBuilder.RenameColumn(name: "ZoneId", table: "sensor", newName: "zone_id");
        migrationBuilder.RenameColumn(name: "Type", table: "sensor", newName: "type");
        migrationBuilder.RenameColumn(name: "Unit", table: "sensor", newName: "unit");
        migrationBuilder.RenameColumn(name: "MinPhysical", table: "sensor", newName: "min_physical");
        migrationBuilder.RenameColumn(name: "MaxPhysical", table: "sensor", newName: "max_physical");
        migrationBuilder.RenameIndex(name: "IX_sensor_ZoneId", table: "sensor", newName: "IX_sensor_zone_id");

        migrationBuilder.RenameColumn(name: "SensorId", table: "sensor_reading", newName: "sensor_id");
        migrationBuilder.RenameColumn(name: "Timestamp", table: "sensor_reading", newName: "timestamp");
        migrationBuilder.RenameColumn(name: "Value", table: "sensor_reading", newName: "value");

        migrationBuilder.RenameColumn(name: "ZoneId", table: "active_thresholds", newName: "zone_id");
        migrationBuilder.RenameColumn(name: "SensorType", table: "active_thresholds", newName: "sensor_type");
        migrationBuilder.RenameColumn(name: "MinValue", table: "active_thresholds", newName: "min_value");
        migrationBuilder.RenameColumn(name: "MaxValue", table: "active_thresholds", newName: "max_value");

        migrationBuilder.RenameColumn(name: "ZoneId", table: "threshold_breach_tracker", newName: "zone_id");
        migrationBuilder.RenameColumn(name: "SensorId", table: "threshold_breach_tracker", newName: "sensor_id");
        migrationBuilder.RenameColumn(name: "SensorType", table: "threshold_breach_tracker", newName: "sensor_type");
        migrationBuilder.RenameColumn(name: "ConsecutiveCount", table: "threshold_breach_tracker", newName: "consecutive_count");
        migrationBuilder.RenameColumn(name: "LastEvaluatedAt", table: "threshold_breach_tracker", newName: "last_evaluated_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(name: "id", table: "sensor", newName: "Id");
        migrationBuilder.RenameColumn(name: "device_id", table: "sensor", newName: "DeviceId");
        migrationBuilder.RenameColumn(name: "zone_id", table: "sensor", newName: "ZoneId");
        migrationBuilder.RenameColumn(name: "type", table: "sensor", newName: "Type");
        migrationBuilder.RenameColumn(name: "unit", table: "sensor", newName: "Unit");
        migrationBuilder.RenameColumn(name: "min_physical", table: "sensor", newName: "MinPhysical");
        migrationBuilder.RenameColumn(name: "max_physical", table: "sensor", newName: "MaxPhysical");
        migrationBuilder.RenameIndex(name: "IX_sensor_zone_id", table: "sensor", newName: "IX_sensor_ZoneId");

        migrationBuilder.RenameColumn(name: "sensor_id", table: "sensor_reading", newName: "SensorId");
        migrationBuilder.RenameColumn(name: "timestamp", table: "sensor_reading", newName: "Timestamp");
        migrationBuilder.RenameColumn(name: "value", table: "sensor_reading", newName: "Value");

        migrationBuilder.RenameColumn(name: "zone_id", table: "active_thresholds", newName: "ZoneId");
        migrationBuilder.RenameColumn(name: "sensor_type", table: "active_thresholds", newName: "SensorType");
        migrationBuilder.RenameColumn(name: "min_value", table: "active_thresholds", newName: "MinValue");
        migrationBuilder.RenameColumn(name: "max_value", table: "active_thresholds", newName: "MaxValue");

        migrationBuilder.RenameColumn(name: "zone_id", table: "threshold_breach_tracker", newName: "ZoneId");
        migrationBuilder.RenameColumn(name: "sensor_id", table: "threshold_breach_tracker", newName: "SensorId");
        migrationBuilder.RenameColumn(name: "sensor_type", table: "threshold_breach_tracker", newName: "SensorType");
        migrationBuilder.RenameColumn(name: "consecutive_count", table: "threshold_breach_tracker", newName: "ConsecutiveCount");
        migrationBuilder.RenameColumn(name: "last_evaluated_at", table: "threshold_breach_tracker", newName: "LastEvaluatedAt");
    }
}
