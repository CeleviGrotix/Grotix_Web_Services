#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

public partial class AssociationInviteEmail : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "InviteEmail",
            table: "association_invite",
            type: "varchar(100)",
            maxLength: 100,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.Sql(
            "UPDATE association_invite SET InviteEmail = CONCAT('legacy-', InviteID, '@invalid.local') WHERE InviteEmail IS NULL");

        migrationBuilder.AlterColumn<string>(
            name: "InviteEmail",
            table: "association_invite",
            type: "varchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(100)",
            oldMaxLength: 100,
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "InviteEmail",
            table: "association_invite");
    }
}
