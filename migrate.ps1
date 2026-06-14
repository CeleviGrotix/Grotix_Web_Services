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

    # 1. Forzamos las variables de entorno en la sesión actual
    $env:TokenSettings__Secret="EstaEsUnaClaveSuperSecretaParaGrotix2024"
    $env:ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=grotix_core;Uid=root;Pwd=root;"
    $env:ConnectionStrings__TelemetryTimescale="Host=localhost;Port=5432;Database=grotix_telemetry;Username=postgres;Password=Grotix2026!;"

    # 2. Elegimos la cadena de conexión según el Label
    $selectedConn = "Server=127.0.0.1;Port=3306;Database=grotix_core;Uid=root;Pwd=root;"
    if ($Label -like "*Telemetry*") {
        $selectedConn = "Host=localhost;Port=5432;Database=grotix_telemetry;Username=postgres;Password=Grotix2026!;"
    }

    # 3. Armamos los argumentos
    $cmdArgs = @(
        "ef", "database", "update",
        "--context", $Context,
        "--project", $Project,
        "--connection", $selectedConn
    )

    if ($StartupProject) {
        $cmdArgs += "--startup-project"
        $cmdArgs += $StartupProject
    }

    # 4. Ejecutamos
    & dotnet @cmdArgs

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
    -Project "src/Grotix.Persistence.HardwareDevice/Grotix.Persistence.HardwareDevice.csproj"

if ($failed) { exit 1 }

Run-Migration `
    -Label "5/6 IrrigationCycle" `
    -Context "IrrigationCycleDbContext" `
    -Project "src/Grotix.Persistence.IrrigationCycle/Grotix.Persistence.IrrigationCycle.csproj"

if ($failed) { exit 1 }

Run-Migration `
    -Label "6/6 Telemetry" `
    -Context "TelemetryDbContext" `
    -Project "src/Grotix.Persistence.Telemetry/Grotix.Persistence.Telemetry.csproj" `
    -StartupProject "src/Telemetry.Api/Telemetry.Api.csproj"

if ($failed) { exit 1 }

Write-Host ""
Write-Host "Todas las migraciones aplicadas correctamente." -ForegroundColor Green
