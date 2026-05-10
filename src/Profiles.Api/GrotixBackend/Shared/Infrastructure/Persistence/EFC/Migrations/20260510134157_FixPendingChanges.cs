using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_identity_identity_HashedPassword_IdentityId",
                table: "identity");

            migrationBuilder.DropForeignKey(
                name: "FK_role_permission_permission_PermissionID",
                table: "role_permission");

            migrationBuilder.DropForeignKey(
                name: "FK_role_permission_role_RoleID",
                table: "role_permission");

            migrationBuilder.DropColumn(
                name: "HashedPassword_IdentityId",
                table: "identity");

            migrationBuilder.AlterColumn<int>(
                name: "ZoneID",
                table: "zone",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "user",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "StaffID",
                table: "staff",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "RoleID",
                table: "role",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "PermissionID",
                table: "permission",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "IdentityID",
                table: "identity",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "FarmID",
                table: "farm",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "CropID",
                table: "crop",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ContractID",
                table: "contract",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "InviteID",
                table: "association_invite",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "AssociationID",
                table: "association",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateIndex(
                name: "IX_zone_CropID",
                table: "zone",
                column: "CropID");

            migrationBuilder.CreateIndex(
                name: "IX_zone_FarmID",
                table: "zone",
                column: "FarmID");

            migrationBuilder.CreateIndex(
                name: "IX_user_AssociationID",
                table: "user",
                column: "AssociationID");

            migrationBuilder.CreateIndex(
                name: "IX_user_RoleID",
                table: "user",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_staff_UserID",
                table: "staff",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_role_permission_PermissionID",
                table: "role_permission",
                column: "PermissionID");

            migrationBuilder.CreateIndex(
                name: "IX_farm_UserID",
                table: "farm",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_contract_AssociationID",
                table: "contract",
                column: "AssociationID");

            migrationBuilder.CreateIndex(
                name: "IX_association_invite_AssociationID",
                table: "association_invite",
                column: "AssociationID");

            migrationBuilder.CreateIndex(
                name: "IX_association_invite_CreatedByUserID",
                table: "association_invite",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_association_invite_RoleID",
                table: "association_invite",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_role_permission_permission_PermissionID",
                table: "role_permission",
                column: "PermissionID",
                principalTable: "permission",
                principalColumn: "PermissionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_role_permission_role_RoleID",
                table: "role_permission",
                column: "RoleID",
                principalTable: "role",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_role_permission_permission_PermissionID",
                table: "role_permission");

            migrationBuilder.DropForeignKey(
                name: "FK_role_permission_role_RoleID",
                table: "role_permission");

            migrationBuilder.DropIndex(
                name: "IX_zone_CropID",
                table: "zone");

            migrationBuilder.DropIndex(
                name: "IX_zone_FarmID",
                table: "zone");

            migrationBuilder.DropIndex(
                name: "IX_user_AssociationID",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_user_RoleID",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_staff_UserID",
                table: "staff");

            migrationBuilder.DropIndex(
                name: "IX_role_permission_PermissionID",
                table: "role_permission");

            migrationBuilder.DropIndex(
                name: "IX_farm_UserID",
                table: "farm");

            migrationBuilder.DropIndex(
                name: "IX_contract_AssociationID",
                table: "contract");

            migrationBuilder.DropIndex(
                name: "IX_association_invite_AssociationID",
                table: "association_invite");

            migrationBuilder.DropIndex(
                name: "IX_association_invite_CreatedByUserID",
                table: "association_invite");

            migrationBuilder.DropIndex(
                name: "IX_association_invite_RoleID",
                table: "association_invite");

            migrationBuilder.AlterColumn<int>(
                name: "ZoneID",
                table: "zone",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "user",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "StaffID",
                table: "staff",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "RoleID",
                table: "role",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "PermissionID",
                table: "permission",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "IdentityID",
                table: "identity",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "HashedPassword_IdentityId",
                table: "identity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "FarmID",
                table: "farm",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "CropID",
                table: "crop",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ContractID",
                table: "contract",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "InviteID",
                table: "association_invite",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "AssociationID",
                table: "association",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_identity_identity_HashedPassword_IdentityId",
                table: "identity",
                column: "HashedPassword_IdentityId",
                principalTable: "identity",
                principalColumn: "IdentityID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_role_permission_permission_PermissionID",
                table: "role_permission",
                column: "PermissionID",
                principalTable: "permission",
                principalColumn: "PermissionID");

            migrationBuilder.AddForeignKey(
                name: "FK_role_permission_role_RoleID",
                table: "role_permission",
                column: "RoleID",
                principalTable: "role",
                principalColumn: "RoleID");
        }
    }
}
