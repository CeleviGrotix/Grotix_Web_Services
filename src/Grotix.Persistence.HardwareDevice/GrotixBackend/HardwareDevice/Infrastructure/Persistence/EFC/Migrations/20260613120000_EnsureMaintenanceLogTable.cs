using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Migrations;

/// <summary>
/// Idempotent repair for environments where <c>20260528150000_AddDiagramMaintenanceTables</c>
/// failed silently during startup (e.g. duplicate column on <c>sensor.LastSeen</c>).
/// </summary>
[DbContext(typeof(HardwareDeviceDbContext))]
[Migration("20260613120000_EnsureMaintenanceLogTable")]
public partial class EnsureMaintenanceLogTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE IF NOT EXISTS `maintenance_log` (
                `LogID` int NOT NULL AUTO_INCREMENT,
                `DeviceID` int NOT NULL,
                `UserID` int NOT NULL,
                `Action` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
                `StatusAfter` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                `Timestamp` datetime(6) NOT NULL,
                PRIMARY KEY (`LogID`),
                KEY `IX_maintenance_log_DeviceID` (`DeviceID`),
                KEY `IX_maintenance_log_UserID` (`UserID`)
            ) CHARACTER SET=utf8mb4;
            """);

        migrationBuilder.Sql(
            """
            CREATE TABLE IF NOT EXISTS `technical_maintenance` (
                `MaintenanceID` int NOT NULL AUTO_INCREMENT,
                `StaffID` int NOT NULL,
                `DeviceID` int NOT NULL,
                `Type` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
                `Description` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
                `Date` datetime(6) NOT NULL,
                `Results` varchar(2000) CHARACTER SET utf8mb4 NULL,
                PRIMARY KEY (`MaintenanceID`),
                KEY `IX_technical_maintenance_DeviceID` (`DeviceID`),
                KEY `IX_technical_maintenance_StaffID` (`StaffID`)
            ) CHARACTER SET=utf8mb4;
            """);

        migrationBuilder.Sql(
            """
            CREATE TABLE IF NOT EXISTS `action_queue` (
                `ActionID` int NOT NULL AUTO_INCREMENT,
                `ActuatorID` int NOT NULL,
                `Command` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                `Status` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                `CreatedAt` datetime(6) NOT NULL,
                PRIMARY KEY (`ActionID`),
                KEY `IX_action_queue_ActuatorID_Status` (`ActuatorID`, `Status`)
            ) CHARACTER SET=utf8mb4;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Repair migration: no-op on rollback.
    }
}
