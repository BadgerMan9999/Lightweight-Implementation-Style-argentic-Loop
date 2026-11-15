#!/usr/bin/env pwsh

# DOOM REMAKE - Project Verification Script
# This script verifies the project structure and configuration

$ErrorActionPreference = "Stop"

Write-Host "=== DOOM REMAKE PROJECT VERIFICATION ===" -ForegroundColor Cyan
Write-Host ""

$verificationResults = @{
    Passed = @()
    Failed = @()
}

function Verify-Item {
    param(
        string $Name,
        string $Path,
        [switch] $MustExist,
        [string]$ExpectedContent
    )
    
    if ($MustExists -and (Test-Path $Path)) {
        Write-Host "[PASS] $Name" -ForegroundColor Green
        $verificationResults.Passed += $Name
        
        if ($ExpectedContent) {
            try {
                $content = Get-Content -Path $Path -Raw -Encoding UTF8
                if ($content -match [regex]::Escape($ExpectedContent)) {
                    Write-Host "       Content verified" -ForegroundColor Gray
                } else {
                    Write-Host "[WARN] Expected content not found in file" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "[ERROR] Could not read file: $_" -ForegroundColor Red
            }
        }
    } elseif (-not (Test-Path $Path)) {
        if ($MustExist) {
            Write-Host "[FAIL] $Name - Path does not exist: $Path" -ForegroundColor Red
            $verificationResults.Failed += "$Name: $Path"
        } else {
            Write-Host "[INFO] $Name - Optional item not found: $Path" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[PASS] $Name exists but optional check skipped" -ForegroundColor Green
    }
}

# Verify directory structure
Write-Host "Verifying directory structure..." -ForegroundColor White
Verify-Item "src/" "src" $true
Verify-Item "assets/" "assets" $false
Verify-Item "docs/" "docs" $false
Verify-Item "logs/" "logs" $false
Verify-Item "DMI/" "DMI" $false
Verify-Item "WOI/" "WOI" $false
Verify-Item "WOutput/" "WOutput" $false

Write-Host ""

# Verify source files
Write-Host "Verifying source files..." -ForegroundColor White
Verify-Item "DoomRemake.csproj" "src\DoomRemake.csproj" $true "net9.0"
Verify-Item "Program.cs" "src\Program.cs" $true
Verify-Item "InputHooks.cs" "src\Core\InputHooks.cs" $false
Verify-Item "WadFileHandler.cs" "src\Core\WadFileHandler.cs" $false
Verify-Item "WadLoader.cs" "src\Core\WadLoader.cs" $false

Write-Host ""

# Verify documentation files
Write-Host "Verifying documentation..." -ForegroundColor White
Verify-Item "README.md" "README.md" $true
Verify-Item ".gitignore" ".gitignore" $true
Verify-Item "SHARDING_PROTOCOL.lisl" "docs\LISL_Specs\SHARDING_PROTOCOL.lisl" $false
Verify-Item "INITIALIZATION_SPEC.lisl" "docs\LISL_Specs\INITIALIZATION_SPEC.lisl" $false

Write-Host ""

# Verify LISL protocol files
Write-Host "Verifying LISL protocol implementation..." -ForegroundColor White
Verify-Item "DoomRemake_Spec_01.xml" "WOI\DoomRemake_Spec_01.xml" $false "[EMITTER:DEV_MANAGER_12B_QAT]"
Verify-Item "Finished.md" "WOutput\Finished.md" $false "[EMITTER:E2B_WORKER_01]"

Write-Host ""

# Summary
$passedCount = $verificationResults.Passed.Count
$failedCount = $verificationResults.Failed.Count

Write-Host "=== VERIFICATION SUMMARY ===" -ForegroundColor Cyan
Write-Host "Passed: $passedCount" -ForegroundColor Green
if ($failedCount -gt 0) {
    Write-Host "Failed: $failedCount" -ForegroundColor Red
    
    if ($failedCount -le 5) {
        Write-Host "`nFailed items:" -ForegroundColor Yellow
        foreach ($item in $verificationResults.Failed) {
            Write-Host "  - $item" -ForegroundColor Red
        }
    }
} else {
    Write-Host "All verification checks passed!" -ForegroundColor Green
}

Write-Host ""

# Final status
if ($failedCount -eq 0) {
    Write-Host "STATUS: PROJECT_READY_FOR_DEVELOPMENT" -ForegroundColor Green
    exit 0
} else {
    Write-Host "STATUS: INCOMPLETE_VERIFICATION" -ForegroundColor Red
    exit 1
}