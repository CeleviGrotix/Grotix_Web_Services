# ============================================================
# Grotix — Verificar si los servicios responden
# Uso:
#   .\check-services.ps1              # local (localhost 5100-5105)
#   .\check-services.ps1 -Azure     # App Services en Azure
#   .\check-services.ps1 -TimeoutSec 60
# ============================================================

param(
    [switch]$Azure,
    [int]$TimeoutSec = 45
)

$ErrorActionPreference = "Continue"

function Test-GrotixEndpoint {
    param(
        [string]$Label,
        [string]$Url
    )

    $metrics = curl.exe -s -o NUL -w "%{http_code} %{time_total}" --max-time $TimeoutSec $Url 2>$null
    $parts = $metrics -split " ", 2
    $code = if ($parts.Count -ge 1) { $parts[0].Trim() } else { "" }
    $time = if ($parts.Count -ge 2) { $parts[1].Trim() } else { "?" }

    if ($code -match "^\d{3}$") {
        if ($code -eq "000") {
            Write-Host ("{0,-22} HTTP {1,-4} {2,6}s  NO CONN (nada escuchando en ese host/puerto)" -f $Label, $code, $time) -ForegroundColor Red
            return $false
        }

        $ok = [int]$code -lt 500
        $color = if ($ok) { "Green" } else { "Red" }
        $status = if ($ok) { "OK" } else { "FAIL" }
        Write-Host ("{0,-22} HTTP {1,-4} {2,6}s  {3}" -f $Label, $code, $time, $status) -ForegroundColor $color
        return $ok
    }

    Write-Host ("{0,-22} HTTP ---- {1,6}s  TIMEOUT" -f $Label, $time) -ForegroundColor Red
    return $false
}

if ($Azure) {
    Write-Host "=== Azure App Services ===" -ForegroundColor Cyan
    $checks = @(
        @{ Label = "Gateway";       Url = "https://grotixgateway1-hrftg6a4gqf0fqhd.chilecentral-01.azurewebsites.net/live" },
        @{ Label = "Profiles";      Url = "https://grotixprofile-byc3drb9gqe9epev.chilecentral-01.azurewebsites.net/live" },
        @{ Label = "Cultivation";   Url = "https://grotixcultivationarea-c5d8hhd3c2defwda.chilecentral-01.azurewebsites.net/live" },
        @{ Label = "Telemetry";     Url = "https://grotixtelemetry-amakfshkb4ahbsbm.chilecentral-01.azurewebsites.net/api/v1/telemetry/health/live" },
        @{ Label = "Hardware";      Url = "https://grotixhardware-dsfucydsavcyhffw.chilecentral-01.azurewebsites.net/api/v1/hardware/health/live" },
        @{ Label = "Irrigation";    Url = "https://grotixirrigationcycle-enbkhfe7a4cye2cm.chilecentral-01.azurewebsites.net/api/v1/irrigation/health/live" }
    )
} else {
    Write-Host "=== Local (dotnet run / start.ps1) ===" -ForegroundColor Cyan
    $checks = @(
        @{ Label = "Gateway";       Url = "http://localhost:5100/live" },
        @{ Label = "Profiles";      Url = "http://localhost:5101/live" },
        @{ Label = "Cultivation";   Url = "http://localhost:5102/live" },
        @{ Label = "Telemetry";     Url = "http://localhost:5103/api/v1/telemetry/health/live" },
        @{ Label = "Hardware";      Url = "http://localhost:5104/api/v1/hardware/health/live" },
        @{ Label = "Irrigation";    Url = "http://localhost:5105/api/v1/irrigation/health/live" }
    )
}

Write-Host ("Timeout: {0}s por servicio`n" -f $TimeoutSec) -ForegroundColor DarkGray

$results = @()
foreach ($c in $checks) {
    $results += Test-GrotixEndpoint -Label $c.Label -Url $c.Url
}

Write-Host ""
$up = ($results | Where-Object { $_ }).Count
$total = $results.Count
if ($up -eq $total) {
    Write-Host "Todos responden ($up/$total)." -ForegroundColor Green
    exit 0
}

Write-Host "Caidos o lentos: $($total - $up) de $total." -ForegroundColor Yellow
Write-Host "HTTP 200/404 = vivo. 503/504 = arrancando o BD caida." -ForegroundColor DarkGray
if (-not $Azure -and $up -eq 0) {
    Write-Host ""
    Write-Host "HTTP 000 en local = no hay dotnet run en localhost:5100-5105." -ForegroundColor Yellow
    Write-Host "Si Swagger lo probaste en Azure, usa:" -ForegroundColor Yellow
    Write-Host "  .\check-services.ps1 -Azure" -ForegroundColor Cyan
}
exit 1
