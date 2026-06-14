param(
    [Parameter(Mandatory)][string]$ProjectPath,
    [Parameter(Mandatory)][string]$AppName,
    [string]$ResourceGroup = "GrotixServices"
)

$ErrorActionPreference = "Stop"

$publishDir = Join-Path $ProjectPath ".publish"
$zipPath    = Join-Path $ProjectPath "deploy.zip"

Write-Host ""
Write-Host "[$AppName] Building..." -ForegroundColor Cyan
dotnet publish $ProjectPath -c Release -r linux-x64 --self-contained false -o $publishDir `
    /p:DebugType=None /p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed." -ForegroundColor Red; exit 1 }

$browserPath = Join-Path $publishDir "runtimes\browser"
if (Test-Path $browserPath) { Remove-Item $browserPath -Recurse -Force }

Write-Host "[$AppName] Compressing..." -ForegroundColor Cyan
Remove-Item $zipPath -ErrorAction SilentlyContinue
Compress-Archive -Path (Get-ChildItem "$publishDir\*" | ForEach-Object { $_.FullName }) `
    -DestinationPath $zipPath -Force

Write-Host "[$AppName] Deploying to Azure (clean)..." -ForegroundColor Cyan
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $AppName `
    --src-path $zipPath `
    --type zip `
    --clean true

if ($LASTEXITCODE -ne 0) { Write-Host "Deploy failed." -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "[$AppName] Done -> https://$AppName.azurewebsites.net" -ForegroundColor Green
