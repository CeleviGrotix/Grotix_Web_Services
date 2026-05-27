#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

public partial class MakeFarmUserIdNullable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "UserID",
            table: "farm",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.Sql("""
            UPDATE farm f
            SET f.UserID = (
                SELECT u.UserID
                FROM user u
                WHERE u.AssociationID = f.AssociationID AND u.RoleID = 3
                ORDER BY u.UserID
                LIMIT 1
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE farm SET UserID = 0 WHERE UserID IS NULL;");

        migrationBuilder.AlterColumn<int>(
            name: "UserID",
            table: "farm",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }
}
