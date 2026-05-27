using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Migrations;

/// <inheritdoc />
[Migration("20260528150000_AddDiagramMaintenanceTables")]
public partial class AddDiagramMaintenanceTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "LastSeen",
            table: "sensor",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "maintenance_log",
            columns: table => new
            {
                LogID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                DeviceID = table.Column<int>(type: "int", nullable: false),
                UserID = table.Column<int>(type: "int", nullable: false),
                Action = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                StatusAfter = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_maintenance_log", x => x.LogID));

        migrationBuilder.CreateTable(
            name: "technical_maintenance",
            columns: table => new
            {
                MaintenanceID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                StaffID = table.Column<int>(type: "int", nullable: false),
                DeviceID = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                Results = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_technical_maintenance", x => x.MaintenanceID));

        migrationBuilder.CreateTable(
            name: "action_queue",
            columns: table => new
            {
                ActionID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ActuatorID = table.Column<int>(type: "int", nullable: false),
                Command = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_action_queue", x => x.ActionID));

        migrationBuilder.CreateIndex(
            name: "IX_maintenance_log_DeviceID",
            table: "maintenance_log",
            column: "DeviceID");

        migrationBuilder.CreateIndex(
            name: "IX_maintenance_log_UserID",
            table: "maintenance_log",
            column: "UserID");

        migrationBuilder.CreateIndex(
            name: "IX_technical_maintenance_DeviceID",
            table: "technical_maintenance",
            column: "DeviceID");

        migrationBuilder.CreateIndex(
            name: "IX_technical_maintenance_StaffID",
            table: "technical_maintenance",
            column: "StaffID");

        migrationBuilder.CreateIndex(
            name: "IX_action_queue_ActuatorID_Status",
            table: "action_queue",
            columns: new[] { "ActuatorID", "Status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "action_queue");
        migrationBuilder.DropTable(name: "technical_maintenance");
        migrationBuilder.DropTable(name: "maintenance_log");
        migrationBuilder.DropColumn(name: "LastSeen", table: "sensor");
    }
}
