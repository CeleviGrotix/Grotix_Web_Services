using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(IrrigationCycleDbContext))]
[Migration("20260528100000_InitialIrrigationCycle")]
public partial class InitialIrrigationCycle : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "irrigation_cycle",
            columns: table => new
            {
                CycleID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", 1),
                ZoneID = table.Column<int>(type: "int", nullable: false),
                StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                VolumeLiters = table.Column<double>(type: "double", nullable: false),
                DurationMinutes = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                AbortReason = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_irrigation_cycle", x => x.CycleID));

        migrationBuilder.CreateIndex(
            name: "IX_irrigation_cycle_ZoneID",
            table: "irrigation_cycle",
            column: "ZoneID");

        migrationBuilder.CreateIndex(
            name: "IX_irrigation_cycle_ZoneID_Status",
            table: "irrigation_cycle",
            columns: new[] { "ZoneID", "Status" });

        migrationBuilder.CreateTable(
            name: "irrigation_schedule",
            columns: table => new
            {
                ScheduleID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", 1),
                ZoneID = table.Column<int>(type: "int", nullable: false),
                DaysOfTheWeek = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                DurationMinutes = table.Column<int>(type: "int", nullable: false),
                isActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_irrigation_schedule", x => x.ScheduleID));

        migrationBuilder.CreateIndex(
            name: "IX_irrigation_schedule_ZoneID",
            table: "irrigation_schedule",
            column: "ZoneID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "irrigation_schedule");
        migrationBuilder.DropTable(name: "irrigation_cycle");
    }
}
