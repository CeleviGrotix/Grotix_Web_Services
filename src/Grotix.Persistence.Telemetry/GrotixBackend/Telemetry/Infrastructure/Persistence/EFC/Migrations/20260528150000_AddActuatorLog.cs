using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Migrations;

public partial class AddActuatorLog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "actuator_log",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                actuator_id = table.Column<int>(type: "integer", nullable: false),
                action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                duration = table.Column<int>(type: "integer", nullable: true),
                timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                flow_rate = table.Column<float>(type: "real", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_actuator_log", x => x.id));

        migrationBuilder.CreateIndex(
            name: "IX_actuator_log_actuator_id_timestamp",
            table: "actuator_log",
            columns: new[] { "actuator_id", "timestamp" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "actuator_log");
}
