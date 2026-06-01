# ============================================================
# Grotix — Aplicar todas las migraciones
# Uso: .\migrate.ps1
# ============================================================

$ErrorActionPreference = "Stop"
$failed = $false

function Run-Migration {
    param(
        [string]$Label,
        [string]$Context,
        [string]$Project,
        [string]$StartupProject
    )

    Write-Host ""
    Write-Host "[$Label]" -ForegroundColor Cyan

    $args = @(
        "ef", "database", "update",
        "--context", $Context,
        "--project", $Project
    )
    if ($StartupProject) {
        $args += "--startup-project", $StartupProject
    }

    & dotnet @args

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FALLO: $Label" -ForegroundColor Red
        $script:failed = $true
    } else {
        Write-Host "  OK: $Label" -ForegroundColor Green
    }
}

Run-Migration `
    -Label "1/6 Profiles" `
    -Context "ProfilesDbContext" `
    -Project "src/Profiles.Api/Profiles.Api.csproj"

if ($failed) { exit 1 }

Run-Migration `
    -Label "2/6 IAM" `
    -Context "IamDbContext" `
    -Project "src/Profiles.Api/Profiles.Api.csproj"

if ($failed) { exit 1 }

Run-Migration `
    -Label "3/6 CultivationArea" `
    -Context "CultivationAreaDbContext" `
    -Project "src/Profiles.Api/Profiles.Api.csproj"

if ($failed) { exit 1 }

Run-Migration `
    -Label "4/6 HardwareDevice" `
    -Context "HardwareDeviceDbContext" `
    -Project "src/Grotix.Persistence.HardwareDevice/Grotix.Persistence.HardwareDevice.csproj" `
    -StartupProject "src/HardwareDevice.Api/HardwareDevice.Api.csproj"

if ($failed) { exit 1 }

Run-Migration `
    -Label "5/6 IrrigationCycle" `
    -Context "IrrigationCycleDbContext" `
    -Project "src/Grotix.Persistence.IrrigationCycle/Grotix.Persistence.IrrigationCycle.csproj" `
    -StartupProject "src/IrrigationCycle.Api/IrrigationCycle.Api.csproj"

if ($failed) { exit 1 }

Run-Migration `
    -Label "6/6 Telemetry" `
    -Context "TelemetryDbContext" `
    -Project "src/Grotix.Persistence.Telemetry/Grotix.Persistence.Telemetry.csproj" `
    -StartupProject "src/Telemetry.Api/Telemetry.Api.csproj"

if ($failed) { exit 1 }

Write-Host ""
Write-Host "Todas las migraciones aplicadas correctamente." -ForegroundColor Green
