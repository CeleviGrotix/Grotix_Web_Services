using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

/// <inheritdoc />
[Migration("20260528150000_AddAnalysisReport")]
public partial class AddAnalysisReport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "analysis_report",
            columns: table => new
            {
                ReportID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ZoneID = table.Column<int>(type: "int", nullable: false),
                DetectedPhase = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                HealthScore = table.Column<float>(type: "float", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_analysis_report", x => x.ReportID);
                table.ForeignKey(
                    name: "FK_analysis_report_zone_ZoneID",
                    column: x => x.ZoneID,
                    principalTable: "zone",
                    principalColumn: "ZoneID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_analysis_report_ZoneID",
            table: "analysis_report",
            column: "ZoneID");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "analysis_report");
}
