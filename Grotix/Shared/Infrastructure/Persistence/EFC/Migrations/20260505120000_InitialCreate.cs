#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

/// <summary>Esquema inicial: IAM (<c>identity</c>), perfiles (<c>user</c>, <c>role</c>, …) y Cultivation Area.</summary>
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "identity",
            columns: table => new
            {
                IdentityID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_identity", x => x.IdentityID);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "role",
            columns: table => new
            {
                RoleID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_role", x => x.RoleID);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "permission",
            columns: table => new
            {
                PermissionID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_permission", x => x.PermissionID);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "association",
            columns: table => new
            {
                AssociationID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_association", x => x.AssociationID);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "crop",
            columns: table => new
            {
                CropID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CommonName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ScientificName = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OptimalTemperature = table.Column<double>(type: "double", nullable: false),
                OptimalHumidity = table.Column<double>(type: "double", nullable: false),
                OptimalLight = table.Column<double>(type: "double", nullable: false),
                MaxStressTime = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_crop", x => x.CropID);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "role_permission",
            columns: table => new
            {
                RoleID = table.Column<int>(type: "int", nullable: false),
                PermissionID = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_role_permission", x => new { x.RoleID, x.PermissionID });
                table.ForeignKey(
                    name: "FK_role_permission_permission_PermissionID",
                    column: x => x.PermissionID,
                    principalTable: "permission",
                    principalColumn: "PermissionID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_role_permission_role_RoleID",
                    column: x => x.RoleID,
                    principalTable: "role",
                    principalColumn: "RoleID",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "user",
            columns: table => new
            {
                UserID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                IdentityID = table.Column<int>(type: "int", nullable: false),
                RoleID = table.Column<int>(type: "int", nullable: false),
                Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Name = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TaxID = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                AssociationID = table.Column<int>(type: "int", nullable: true),
                profilePicture = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Preferences = table.Column<string>(type: "json", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user", x => x.UserID);
                table.ForeignKey(
                    name: "FK_user_association_AssociationID",
                    column: x => x.AssociationID,
                    principalTable: "association",
                    principalColumn: "AssociationID",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_user_role_RoleID",
                    column: x => x.RoleID,
                    principalTable: "role",
                    principalColumn: "RoleID",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "farm",
            columns: table => new
            {
                FarmID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserID = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Location = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_farm", x => x.FarmID);
                table.ForeignKey(
                    name: "FK_farm_user_UserID",
                    column: x => x.UserID,
                    principalTable: "user",
                    principalColumn: "UserID",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "staff",
            columns: table => new
            {
                StaffID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserID = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                TechnicalRole = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_staff", x => x.StaffID);
                table.ForeignKey(
                    name: "FK_staff_user_UserID",
                    column: x => x.UserID,
                    principalTable: "user",
                    principalColumn: "UserID",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "zone",
            columns: table => new
            {
                ZoneID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                FarmID = table.Column<int>(type: "int", nullable: false),
                CropID = table.Column<int>(type: "int", nullable: false),
                CurrentPhase = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PhaseStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ImageURL = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Latitude = table.Column<double>(type: "double", nullable: false),
                Longitude = table.Column<double>(type: "double", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_zone", x => x.ZoneID);
                table.ForeignKey(
                    name: "FK_zone_crop_CropID",
                    column: x => x.CropID,
                    principalTable: "crop",
                    principalColumn: "CropID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_zone_farm_FarmID",
                    column: x => x.FarmID,
                    principalTable: "farm",
                    principalColumn: "FarmID",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_farm_UserID",
            table: "farm",
            column: "UserID");

        migrationBuilder.CreateIndex(
            name: "IX_role_permission_PermissionID",
            table: "role_permission",
            column: "PermissionID");

        migrationBuilder.CreateIndex(
            name: "IX_staff_UserID",
            table: "staff",
            column: "UserID");

        migrationBuilder.CreateIndex(
            name: "IX_user_AssociationID",
            table: "user",
            column: "AssociationID");

        migrationBuilder.CreateIndex(
            name: "IX_user_RoleID",
            table: "user",
            column: "RoleID");

        migrationBuilder.CreateIndex(
            name: "IX_zone_CropID",
            table: "zone",
            column: "CropID");

        migrationBuilder.CreateIndex(
            name: "IX_zone_FarmID",
            table: "zone",
            column: "FarmID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "zone");
        migrationBuilder.DropTable(name: "staff");
        migrationBuilder.DropTable(name: "farm");
        migrationBuilder.DropTable(name: "crop");
        migrationBuilder.DropTable(name: "user");
        migrationBuilder.DropTable(name: "role_permission");
        migrationBuilder.DropTable(name: "association");
        migrationBuilder.DropTable(name: "permission");
        migrationBuilder.DropTable(name: "role");
        migrationBuilder.DropTable(name: "identity");
    }
}
