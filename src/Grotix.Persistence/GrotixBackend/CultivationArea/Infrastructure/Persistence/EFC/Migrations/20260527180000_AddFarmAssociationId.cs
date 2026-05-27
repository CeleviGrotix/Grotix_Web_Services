#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

public partial class AddFarmAssociationId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "AssociationID",
            table: "farm",
            type: "int",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE farm f
            INNER JOIN user u ON u.UserID = f.UserID
            SET f.AssociationID = u.AssociationID
            WHERE u.AssociationID IS NOT NULL;
            """);

        migrationBuilder.Sql("""
            UPDATE farm
            SET AssociationID = 0
            WHERE AssociationID IS NULL;
            """);

        migrationBuilder.AlterColumn<int>(
            name: "AssociationID",
            table: "farm",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_farm_AssociationID",
            table: "farm",
            column: "AssociationID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_farm_AssociationID",
            table: "farm");

        migrationBuilder.DropColumn(
            name: "AssociationID",
            table: "farm");
    }
}
