param(
    [Parameter(Mandatory)][string]$ProjectPath,
    [Parameter(Mandatory)][string]$AppName,
    [string]$ResourceGroup = "GrotixServices"
)




$ErrorActionPreference = "Stop"

function Resolve-ToolPath {
    param(
        [Parameter(Mandatory)][string]$Name,
        [string[]]$Candidates
    )

    $fromPath = Get-Command $Name -ErrorAction SilentlyContinue
    if ($fromPath) { return $fromPath.Source }

    foreach ($candidate in $Candidates) {
        if (Test-Path $candidate) { return $candidate }
    }

    throw "No se encontró '$Name'. Instálalo o agrégalo al PATH."
}

function New-LinuxZip {
    param(
        [Parameter(Mandatory)][string]$SourceDir,
        [Parameter(Mandatory)][string]$DestinationPath
    )

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $sourceFull = (Resolve-Path $SourceDir).Path.TrimEnd('\')
    $destFull = [System.IO.Path]::GetFullPath($DestinationPath)
    if (Test-Path $destFull) { Remove-Item $destFull -Force }

    $zip = [System.IO.Compression.ZipFile]::Open($destFull, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        Get-ChildItem $sourceFull -Recurse -File | ForEach-Object {
            $entryName = $_.FullName.Substring($sourceFull.Length + 1).Replace('\', '/')
            [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $entryName)
        }
    }
    finally {
        $zip.Dispose()
    }
}

$dotnet = Resolve-ToolPath -Name "dotnet" -Candidates @(
    "$env:ProgramFiles\dotnet\dotnet.exe",
    "${env:ProgramFiles(x86)}\dotnet\dotnet.exe",
    "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
)

$az = Resolve-ToolPath -Name "az" -Candidates @(
    "$env:ProgramFiles\Microsoft SDKs\Azure\CLI2\wbin\az.cmd",
    "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
)

$ProjectPath = (Resolve-Path $ProjectPath).Path
$publishDir = Join-Path $ProjectPath ".publish"
$zipPath    = Join-Path $ProjectPath "deploy.zip"

Write-Host ""
Write-Host "[$AppName] Building..." -ForegroundColor Cyan
& $dotnet publish $ProjectPath -c Release -r linux-x64 --self-contained false -o $publishDir `
    /p:DebugType=None /p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed." -ForegroundColor Red; exit 1 }

$browserPath = Join-Path $publishDir "runtimes\browser"
if (Test-Path $browserPath) { Remove-Item $browserPath -Recurse -Force }

Write-Host "[$AppName] Compressing (unix paths)..." -ForegroundColor Cyan
New-LinuxZip -SourceDir $publishDir -DestinationPath $zipPath

Write-Host "[$AppName] Deploying to Azure (async, no espera arranque del sitio)..." -ForegroundColor Cyan
& $az webapp deploy `
    --resource-group $ResourceGroup `
    --name $AppName `
    --src-path $zipPath `
    --type zip `
    --clean true `
    --async true `
    --timeout 600000

if ($LASTEXITCODE -ne 0) { Write-Host "Deploy failed." -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "[$AppName] Done -> https://$AppName.azurewebsites.net" -ForegroundColor Green
