#!/usr/bin/env pwsh

# DOOM REMAKE - Project Initialization Script
# This script automates the project structure creation and verification

$ErrorActionPreference = "Stop"

Write-Host "=== DOOM REMAKE PROJECT INITIALIZATION ===" -ForegroundColor Cyan
Write-Host ""

# Function to create directory if it doesn't exist
function Create-Directory {
    param(string $Path)
    
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
        Write-Host "[OK] Created: $Path" -ForegroundColor Green
    } else {
        Write-Host "[SKIP] Exists: $Path" -ForegroundColor Yellow
    }
}

# Function to create file with content
function Create-File {
    param(
        string $Path,
        string $Content
    )
    
    if (-not (Test-Path $Path)) {
        Set-Content -Path $Path -Value $Content -Encoding UTF8
        Write-Host "[OK] Created: $Path" -ForegroundColor Green
    } else {
        Write-Host "[SKIP] Exists: $Path" -ForegroundColor Yellow
    }
}

# Create directory structure
Write-Host "Creating directory structure..." -ForegroundColor White
Create-Directory -Path "src"
Create-Directory -Path "assets"
Create-Directory -Path "docs"
Create-Directory -Path "logs"
Create-Directory -Path "Core"

Write-Host ""
Write-Host "Project structure created successfully!" -ForegroundColor Green
Write-Host ""

# Verify .NET SDK is available
Write-Host "Checking for .NET SDK..." -ForegroundColor White
try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] .NET SDK Version: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "[WARN] .NET SDK not found. Please install .NET 9.0 SDK." -ForegroundColor Yellow
    }
} catch {
    Write-Host "[ERROR] Failed to check .NET SDK: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Project initialization complete!" -ForegroundColor Cyan
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Add DOOM WAD files to assets/wads/" -ForegroundColor Yellow
Write-Host "  2. Run 'dotnet build' to compile the project" -ForegroundColor Yellow
Write-Host "  3. Review LISL specifications in docs/LISL_Specs/" -ForegroundColor Yellow
