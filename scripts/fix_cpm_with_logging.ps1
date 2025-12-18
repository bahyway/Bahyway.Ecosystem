# ============================================================
# CPM Fix Script with Logging - SYNTAX CORRECTED
# ============================================================

$SolutionDir = "C:\Dev\BahyWay"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = Join-Path $SolutionDir "cpm_fix_log_$timestamp.txt"

# Start logging
Start-Transcript -Path $logFile

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CPM FIX SCRIPT WITH LOGGING" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Started: $(Get-Date)" -ForegroundColor Green
Write-Host "Solution: $SolutionDir" -ForegroundColor Green
Write-Host "Log File: $logFile" -ForegroundColor Green
Write-Host ""

# Change directory
Set-Location $SolutionDir

# ============================================================
# STEP 1: Check Files
# ============================================================
Write-Host "[1/7] Checking file locations..." -ForegroundColor Yellow

$packagesProps = "Directory.Packages.props"
$buildProps = "Directory.Build.props"

if (Test-Path $packagesProps) {
    Write-Host "  Found: $packagesProps" -ForegroundColor Green
} else {
    Write-Host "  ERROR: $packagesProps not found!" -ForegroundColor Red
    Stop-Transcript
    exit 1
}

if (Test-Path $buildProps) {
    Write-Host "  Found: $buildProps" -ForegroundColor Green
} else {
    Write-Host "  Creating $buildProps..." -ForegroundColor Yellow
    @'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
</Project>
'@ | Out-File -FilePath $buildProps -Encoding UTF8
    Write-Host "  Created: $buildProps" -ForegroundColor Green
}

# ============================================================
# STEP 2: Verify CPM Enabled
# ============================================================
Write-Host ""
Write-Host "[2/7] Verifying CPM is enabled..." -ForegroundColor Yellow

$buildContent = Get-Content $buildProps -Raw
if ($buildContent -match "ManagePackageVersionsCentrally") {
    Write-Host "  CPM is enabled" -ForegroundColor Green
} else {
    Write-Host "  ERROR: CPM not enabled!" -ForegroundColor Red
    Stop-Transcript
    exit 1
}

# ============================================================
# STEP 3: Find .csproj Files
# ============================================================
Write-Host ""
Write-Host "[3/7] Finding .csproj files..." -ForegroundColor Yellow

$csprojFiles = Get-ChildItem -Recurse -Filter "*.csproj"
Write-Host "  Found $($csprojFiles.Count) project files" -ForegroundColor Green

# ============================================================
# STEP 4: Check for Version Attributes
# ============================================================
Write-Host ""
Write-Host "[4/7] Checking for Version attributes..." -ForegroundColor Yellow

$filesNeedingFix = @()
foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match 'Version="') {
        $filesNeedingFix += $file
        $shortPath = $file.FullName.Replace($SolutionDir, "")
        Write-Host "  Needs fix: $shortPath" -ForegroundColor Yellow
    }
}

if ($filesNeedingFix.Count -eq 0) {
    Write-Host "  No files need fixing" -ForegroundColor Green
} else {
    Write-Host "  Found $($filesNeedingFix.Count) files with Version attributes" -ForegroundColor Yellow
}

# ============================================================
# STEP 5: Remove Version Attributes
# ============================================================
Write-Host ""
Write-Host "[5/7] Removing Version attributes..." -ForegroundColor Yellow

$fixedCount = 0
foreach ($file in $filesNeedingFix) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content -replace 'Version="[^"]*"', ''

    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        $fixedCount++
        $shortPath = $file.FullName.Replace($SolutionDir, "")
        Write-Host "  Fixed: $shortPath" -ForegroundColor Green
    }
}

Write-Host "  Fixed $fixedCount files" -ForegroundColor Green

# ============================================================
# STEP 6: Clear NuGet Cache
# ============================================================
Write-Host ""
Write-Host "[6/7] Clearing NuGet cache..." -ForegroundColor Yellow

dotnet nuget locals all --clear
Write-Host "  Cache cleared" -ForegroundColor Green

# ============================================================
# STEP 7: Delete bin/obj
# ============================================================
Write-Host ""
Write-Host "[7/7] Deleting bin/obj folders..." -ForegroundColor Yellow

$binFolders = Get-ChildItem -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue
$objFolders = Get-ChildItem -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue

Write-Host "  Deleting $($binFolders.Count) bin folders..." -ForegroundColor Cyan
$binFolders | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "  Deleting $($objFolders.Count) obj folders..." -ForegroundColor Cyan
$objFolders | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "  Folders deleted" -ForegroundColor Green

# ============================================================
# STEP 8: Restore
# ============================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESTORING PACKAGES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Running: dotnet restore --force" -ForegroundColor Yellow
Write-Host "This may take several minutes..." -ForegroundColor Yellow
Write-Host ""

$restoreOutput = dotnet restore --force 2>&1
$exitCode = $LASTEXITCODE

Write-Host ""
Write-Host "--- RESTORE OUTPUT ---" -ForegroundColor Cyan
$restoreOutput | ForEach-Object {
    $line = $_.ToString()
    if ($line -match "error") {
        Write-Host $line -ForegroundColor Red
    } elseif ($line -match "warning") {
        Write-Host $line -ForegroundColor Yellow
    } else {
        Write-Host $line
    }
}
Write-Host "--- END OUTPUT ---" -ForegroundColor Cyan

# ============================================================
# SUMMARY
# ============================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($exitCode -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Log saved to: $logFile" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next: dotnet build" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "RESTORE FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host "Exit code: $exitCode" -ForegroundColor Red
    Write-Host "Log saved to: $logFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please share the log file for analysis" -ForegroundColor Yellow
    Write-Host ""
}

Stop-Transcript
exit $exitCode
