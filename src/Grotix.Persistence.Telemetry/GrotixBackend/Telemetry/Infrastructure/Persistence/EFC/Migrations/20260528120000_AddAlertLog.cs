using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(TelemetryDbContext))]
[Migration("20260528120000_AddAlertLog")]
public partial class AddAlertLog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "alert_log",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                zone_id = table.Column<int>(type: "integer", nullable: false),
                sensor_id = table.Column<int>(type: "integer", nullable: false),
                sensor_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                value = table.Column<double>(type: "double precision", nullable: false),
                min_threshold = table.Column<double>(type: "double precision", nullable: false),
                max_threshold = table.Column<double>(type: "double precision", nullable: false),
                breached_threshold = table.Column<double>(type: "double precision", nullable: false),
                breach_direction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                triggered_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_alert_log", x => x.id));

        migrationBuilder.CreateIndex(
            name: "IX_alert_log_zone_id_triggered_at",
            table: "alert_log",
            columns: new[] { "zone_id", "triggered_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "alert_log");
}
