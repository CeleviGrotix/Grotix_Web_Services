# ============================================================
# Grotix — Levantar todos los servicios
# Uso: .\start.ps1
# Cada servicio abre en su propia ventana de PowerShell.
# ============================================================

$root = $PSScriptRoot

$services = @(
    @{ Name = "Profiles      :5101"; Project = "src/Profiles.Api/Profiles.Api.csproj" },
    @{ Name = "Cultivation   :5102"; Project = "src/CultivationArea.Api/CultivationArea.Api.csproj" },
    @{ Name = "Telemetry     :5103"; Project = "src/Telemetry.Api/Telemetry.Api.csproj" },
    @{ Name = "HardwareDevice:5104"; Project = "src/HardwareDevice.Api/HardwareDevice.Api.csproj" },
    @{ Name = "Irrigation    :5105"; Project = "src/IrrigationCycle.Api/IrrigationCycle.Api.csproj" },
    @{ Name = "Gateway       :5100"; Project = "src/Gateway.Api/Gateway.Api.csproj" }
)

foreach ($svc in $services) {
    $cmd = "cd '$root'; dotnet run --project $($svc.Project)"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $cmd
    Write-Host "Iniciando $($svc.Name)..." -ForegroundColor Cyan
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "Todos los servicios iniciados." -ForegroundColor Green
Write-Host "Gateway disponible en: http://localhost:5100" -ForegroundColor Yellow
