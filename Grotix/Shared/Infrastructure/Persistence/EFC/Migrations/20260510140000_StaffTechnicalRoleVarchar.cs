#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

/// <summary>Almacena el rol técnico del staff como <c>varchar</c> en lugar de <c>longtext</c>.</summary>
public partial class StaffTechnicalRoleVarchar : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TechnicalRole",
            table: "staff",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext",
            oldNullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TechnicalRole",
            table: "staff",
            type: "longtext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32,
            oldNullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");
    }
}
