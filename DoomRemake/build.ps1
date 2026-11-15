#!/usr/bin/env pwsh

# DOOM REMAKE - Build Script
# This script builds the project and runs verification tasks

$ErrorActionPreference = "Stop"

Write-Host "=== DOOM REMAKE BUILD SCRIPT ===" -ForegroundColor Cyan
Write-Host ""

$buildSuccess = $true

try {
    # Restore NuGet packages
    Write-Host "Restoring NuGet packages..." -ForegroundColor White
    dotnet restore src/DoomRemake.csproj
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Packages restored successfully" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] Failed to restore packages" -ForegroundColor Red
        $buildSuccess = $false
    }
} catch {
    Write-Host "[ERROR] Restore failed: $_" -ForegroundColor Red
    $buildSuccess = $false
}

if ($buildSuccess) {
    # Build the project
    Write-Host ""
    Write-Host "Building project..." -ForegroundColor White
    
    dotnet build src/DoomRemake.csproj --configuration Release
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Build successful" -ForegroundColor Green
        
        # List output files
        $binPath = Join-Path (Get-Location) "src\DoomRemake\bin\Release\net9.0"
        if (Test-Path $binPath) {
            Write-Host ""
            Write-Host "Output files:" -ForegroundColor White
            Get-ChildItem -Path $binPath -Recurse | ForEach-Object {
                Write-Host "  $_.Name" -ForegroundColor Cyan
            }
        }
    } else {
        Write-Host "[ERROR] Build failed" -ForegroundColor Red
        $buildSuccess = $false
    }
}

if ($buildSuccess) {
    Write-Host ""
    Write-Host "=== BUILD COMPLETE ===" -ForegroundColor Green
    
    # Show project summary
    Write-Host "`nProject Summary:" -ForegroundColor White
    Write-Host "  Target Framework: .NET 9.0" -ForegroundColor Cyan
    Write-Host "  Output Path: src\DoomRemake\bin\Release\net9.0\DoomRemake.exe" -ForegroundColor Cyan
    Write-Host "  Working Directory: $(Get-Location)" -ForegroundColor Cyan
    
    Write-Host "`nReady to run with: dotnet run" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "Build failed. Please check the errors above." -ForegroundColor Red
    exit 1
}