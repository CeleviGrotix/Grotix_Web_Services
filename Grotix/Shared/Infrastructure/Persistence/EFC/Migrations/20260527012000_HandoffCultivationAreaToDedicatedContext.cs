#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

/// <summary>
/// Handoff de ownership del modelo: AppDbContext deja de administrar crop/farm/zone, sin cambios físicos en las tablas.
/// </summary>
public partial class HandoffCultivationAreaToDedicatedContext : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
