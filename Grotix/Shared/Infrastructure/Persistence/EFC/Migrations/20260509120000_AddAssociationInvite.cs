#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

public partial class AddAssociationInvite : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "association_invite",
            columns: table => new
            {
                InviteID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                AssociationID = table.Column<int>(type: "int", nullable: false),
                TokenHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RoleID = table.Column<int>(type: "int", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                UsedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CreatedByUserID = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_association_invite", x => x.InviteID);
                table.ForeignKey(
                    name: "FK_association_invite_association_AssociationID",
                    column: x => x.AssociationID,
                    principalTable: "association",
                    principalColumn: "AssociationID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_association_invite_role_RoleID",
                    column: x => x.RoleID,
                    principalTable: "role",
                    principalColumn: "RoleID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_association_invite_user_CreatedByUserID",
                    column: x => x.CreatedByUserID,
                    principalTable: "user",
                    principalColumn: "UserID",
                    onDelete: ReferentialAction.SetNull);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_association_invite_TokenHash",
            table: "association_invite",
            column: "TokenHash",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "association_invite");
    }
}
