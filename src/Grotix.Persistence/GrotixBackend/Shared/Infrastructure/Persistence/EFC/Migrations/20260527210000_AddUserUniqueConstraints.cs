#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Grotix.Persistence.GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

public partial class AddUserUniqueConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_user_Email",
            table: "user",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_user_IdentityID",
            table: "user",
            column: "IdentityID",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_user_Email",
            table: "user");

        migrationBuilder.DropIndex(
            name: "IX_user_IdentityID",
            table: "user");
    }
}
