#!/usr/bin/env pwsh
# Test script to verify module loading

Write-Host "Testing BahyWay.PostgreSQLHA Module Loading..." -ForegroundColor Cyan
Write-Host ""

# Remove any loaded instances
Remove-Module BahyWay.PostgreSQLHA -Force -ErrorAction SilentlyContinue

# Get the module path
$modulePath = "$PSScriptRoot/src/AlarmInsight.Infrastructure/PowerShellModules/BahyWay.PostgreSQLHA"

Write-Host "Module Path: $modulePath" -ForegroundColor Yellow
Write-Host ""

# Check if files exist
$psd1 = Join-Path $modulePath "BahyWay.PostgreSQLHA.psd1"
$psm1 = Join-Path $modulePath "BahyWay.PostgreSQLHA.psm1"

if (Test-Path $psd1) {
    Write-Host "✅ Manifest file found: $psd1" -ForegroundColor Green
} else {
    Write-Host "❌ Manifest file NOT found: $psd1" -ForegroundColor Red
    exit 1
}

if (Test-Path $psm1) {
    Write-Host "✅ Module file found: $psm1" -ForegroundColor Green
} else {
    Write-Host "❌ Module file NOT found: $psm1" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Check for syntax errors
Write-Host "Checking for syntax errors..." -ForegroundColor Cyan
try {
    $null = [System.Management.Automation.PSParser]::Tokenize((Get-Content $psm1 -Raw), [ref]$null)
    Write-Host "✅ No syntax errors found" -ForegroundColor Green
} catch {
    Write-Host "❌ Syntax error found: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Import the module
Write-Host "Importing module..." -ForegroundColor Cyan
try {
    Import-Module $modulePath -Force -ErrorAction Stop
    Write-Host "✅ Module imported successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to import module: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# List all exported functions
Write-Host "Exported Functions:" -ForegroundColor Cyan
$functions = Get-Command -Module BahyWay.PostgreSQLHA -CommandType Function
$functions | Format-Table -Property Name, Source -AutoSize

Write-Host ""
Write-Host "Total functions exported: $($functions.Count)" -ForegroundColor Yellow

# Check for specific deployment functions
Write-Host ""
Write-Host "Checking for deployment functions..." -ForegroundColor Cyan

$deploymentFunctions = @(
    'Initialize-PostgreSQLHA',
    'Deploy-PostgreSQLCluster',
    'Remove-PostgreSQLCluster',
    'Start-PostgreSQLCluster',
    'Stop-PostgreSQLCluster',
    'Start-PostgreSQLReplication'
)

foreach ($func in $deploymentFunctions) {
    if (Get-Command -Name $func -Module BahyWay.PostgreSQLHA -ErrorAction SilentlyContinue) {
        Write-Host "  ✅ $func" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $func MISSING" -ForegroundColor Red
    }
}

Write-Host ""

# Check what's in the manifest
Write-Host "Functions listed in manifest:" -ForegroundColor Cyan
$manifest = Import-PowerShellDataFile -Path $psd1
Write-Host "  Total in manifest: $($manifest.FunctionsToExport.Count)" -ForegroundColor Yellow
$manifest.FunctionsToExport | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }

Write-Host ""
Write-Host "Done!" -ForegroundColor Cyan
