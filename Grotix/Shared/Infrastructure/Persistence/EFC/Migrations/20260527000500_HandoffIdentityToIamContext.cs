#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

/// <summary>
/// Handoff de ownership del modelo: AppDbContext deja de administrar identity, sin cambios físicos en la tabla.
/// </summary>
public partial class HandoffIdentityToIamContext : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
