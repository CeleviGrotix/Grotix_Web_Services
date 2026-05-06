#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

public partial class SeedDefaultRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO role (RoleID, Name, Description)
            SELECT 1, 'Admin', 'Administrador del sistema'
            WHERE NOT EXISTS (SELECT 1 FROM role WHERE RoleID = 1);
            """);

        migrationBuilder.Sql("""
            INSERT INTO role (RoleID, Name, Description)
            SELECT 2, 'Staff', 'Operador técnico'
            WHERE NOT EXISTS (SELECT 1 FROM role WHERE RoleID = 2);
            """);

        migrationBuilder.Sql("""
            INSERT INTO role (RoleID, Name, Description)
            SELECT 3, 'User', 'Usuario estándar'
            WHERE NOT EXISTS (SELECT 1 FROM role WHERE RoleID = 3);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM role WHERE RoleID IN (1,2,3);");
    }
}
