#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

public partial class AddZoneMemberAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "zone_member",
            columns: table => new
            {
                ZoneMemberID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ZoneID = table.Column<int>(type: "int", nullable: false),
                UserID = table.Column<int>(type: "int", nullable: false),
                AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                AssignedByUserID = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_zone_member", x => x.ZoneMemberID);
                table.ForeignKey(
                    name: "FK_zone_member_zone_ZoneID",
                    column: x => x.ZoneID,
                    principalTable: "zone",
                    principalColumn: "ZoneID",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_zone_member_UserID",
            table: "zone_member",
            column: "UserID");

        migrationBuilder.CreateIndex(
            name: "IX_zone_member_ZoneID_UserID",
            table: "zone_member",
            columns: new[] { "ZoneID", "UserID" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "zone_member");
    }
}
