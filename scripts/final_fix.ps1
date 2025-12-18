# ============================================================
# FINAL FIX - Kill Processes, Delete Locked Files, Restore
# ============================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FINAL CPM FIX - FORCE UNLOCK" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Kill all processes
Write-Host "[1/5] Killing Visual Studio and build processes..." -ForegroundColor Yellow
$processes = @("devenv", "MSBuild", "dotnet", "VBCSCompiler")
foreach ($proc in $processes) {
    $killed = Get-Process -Name $proc -ErrorAction SilentlyContinue
    if ($killed) {
        Stop-Process -Name $proc -Force -ErrorAction SilentlyContinue
        Write-Host "  Killed: $proc" -ForegroundColor Green
    }
}
Start-Sleep -Seconds 3
Write-Host "  Done" -ForegroundColor Green

# Step 2: Delete locked DLL files
Write-Host ""
Write-Host "[2/5] Deleting locked DLL files..." -ForegroundColor Yellow
$lockedFiles = @(
    "C:\Users\Bahaa\.nuget\packages\antlr4buildtasks\12.8.0\lib\netstandard2.0\Antlr4BuildTasks.dll",
    "C:\Users\Bahaa\.nuget\packages\microsoft.visualstudio.javascript.sdk\0.5.128-alpha\tools\Microsoft.VisualStudio.JavaScript.Tasks.dll"
)

foreach ($file in $lockedFiles) {
    if (Test-Path $file) {
        Remove-Item $file -Force -ErrorAction SilentlyContinue
        Write-Host "  Deleted: $(Split-Path $file -Leaf)" -ForegroundColor Green
    }
}

# Step 3: Delete entire problematic package folders
Write-Host ""
Write-Host "[3/5] Deleting problematic package folders..." -ForegroundColor Yellow
$packageFolders = @(
    "C:\Users\Bahaa\.nuget\packages\antlr4buildtasks",
    "C:\Users\Bahaa\.nuget\packages\microsoft.visualstudio.javascript.sdk"
)

foreach ($folder in $packageFolders) {
    if (Test-Path $folder) {
        Remove-Item $folder -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Deleted: $(Split-Path $folder -Leaf)" -ForegroundColor Green
    }
}

# Step 4: Delete all obj folders
Write-Host ""
Write-Host "[4/5] Deleting all obj folders..." -ForegroundColor Yellow
Set-Location C:\Dev\BahyWay
$objFolders = Get-ChildItem -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue
$objCount = $objFolders.Count
Write-Host "  Found $objCount obj folders to delete..." -ForegroundColor Cyan

foreach ($folder in $objFolders) {
    Remove-Item $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "  Deleted $objCount obj folders" -ForegroundColor Green

# Step 5: Restore
Write-Host ""
Write-Host "[5/5] Restoring packages..." -ForegroundColor Yellow
Write-Host "  Running: dotnet restore --force" -ForegroundColor Cyan
Write-Host "  This may take 2-3 minutes..." -ForegroundColor Cyan
Write-Host ""

$output = dotnet restore --force 2>&1
$exitCode = $LASTEXITCODE

# Show output
$output | ForEach-Object {
    $line = $_.ToString()
    if ($line -match "error") {
        Write-Host $line -ForegroundColor Red
    } elseif ($line -match "warning") {
        Write-Host $line -ForegroundColor Yellow
    } elseif ($line -match "Restored") {
        Write-Host $line -ForegroundColor Green
    } else {
        Write-Host $line
    }
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESULT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($exitCode -eq 0) {
    Write-Host ""
    Write-Host "✓✓✓ SUCCESS! ✓✓✓" -ForegroundColor Green
    Write-Host ""
    Write-Host "All packages restored successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next: dotnet build" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "✗✗✗ STILL FAILED ✗✗✗" -ForegroundColor Red
    Write-Host ""
    Write-Host "Exit code: $exitCode" -ForegroundColor Red
    Write-Host ""
    Write-Host "If errors persist, try:" -ForegroundColor Yellow
    Write-Host "  1. Restart your computer" -ForegroundColor White
    Write-Host "  2. Run this script again" -ForegroundColor White
    Write-Host ""
}
