# Run from repo root (folder containing IndustrialPress.sln):
#   powershell -ExecutionPolicy Bypass -File .\scripts\Create-Phase0Zip.ps1
$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$sln = Join-Path $repoRoot "IndustrialPress.sln"
if (-not (Test-Path $sln)) {
    throw "IndustrialPress.sln not found. Run from repo root. repoRoot=$repoRoot"
}

# Architecture doc: prefer repo docs/, else parent C:\Users\zeev\docs\
$arch = Join-Path $repoRoot "docs\architecture.md"
if (-not (Test-Path $arch)) {
    $parentArch = Join-Path (Split-Path $repoRoot -Parent) "docs\architecture.md"
    if (Test-Path $parentArch) {
        New-Item -ItemType Directory -Force -Path (Split-Path $arch) | Out-Null
        Copy-Item $parentArch $arch -Force
        Write-Host "Copied architecture from: $parentArch"
    } else {
        Write-Warning "Missing $arch — add docs\architecture.md before zipping"
    }
} else {
    Write-Host "Including: $arch"
}

$zip = Join-Path $repoRoot "IndustrialPress-Phase0.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }

# Zip repo contents (parent folder name becomes root inside zip)
$folderName = Split-Path $repoRoot -Leaf
$parent = Split-Path $repoRoot -Parent
$tempZip = Join-Path $parent "$folderName-Phase0.zip"
Compress-Archive -Path $repoRoot -DestinationPath $tempZip -Force
Move-Item $tempZip $zip -Force

$desk = Join-Path $env:USERPROFILE "Desktop\IndustrialPress-Phase0.zip"
if (Test-Path (Split-Path $desk)) {
    Copy-Item $zip $desk -Force
    Write-Host "Desktop copy: $desk"
}

Write-Host "Created: $zip"
Write-Host "Size:    $((Get-Item $zip).Length) bytes"
