#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

public partial class UserIsActive : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "user",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "user");
    }
}
