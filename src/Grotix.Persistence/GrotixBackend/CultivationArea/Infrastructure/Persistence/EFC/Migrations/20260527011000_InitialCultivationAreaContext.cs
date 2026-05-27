#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

/// <summary>
/// Baseline de CultivationArea sobre una base existente: solo toma ownership lógico del esquema crop/farm/zone.
/// </summary>
public partial class InitialCultivationAreaContext : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
