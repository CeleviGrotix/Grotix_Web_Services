#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

public partial class AddFarmAndCropUniqueConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_crop_CommonName",
            table: "crop",
            column: "CommonName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_crop_ScientificName",
            table: "crop",
            column: "ScientificName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_farm_AssociationID_Name",
            table: "farm",
            columns: new[] { "AssociationID", "Name" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_crop_CommonName",
            table: "crop");

        migrationBuilder.DropIndex(
            name: "IX_crop_ScientificName",
            table: "crop");

        migrationBuilder.DropIndex(
            name: "IX_farm_AssociationID_Name",
            table: "farm");
    }
}
