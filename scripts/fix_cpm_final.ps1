# ============================================================
# Final CPM Fix - CORRECTED VERSION
# ============================================================

$solutionDir = "C:\Dev\BahyWay"
Set-Location $solutionDir

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FINAL CPM FIX" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Verify file locations
Write-Host ""
Write-Host "[1/6] Checking file locations..." -ForegroundColor Yellow

$packagesProps = Join-Path $solutionDir "Directory.Packages.props"
$buildProps = Join-Path $solutionDir "Directory.Build.props"

if (-not (Test-Path $packagesProps)) {
    Write-Host "  ERROR: Directory.Packages.props not found!" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $buildProps)) {
    Write-Host "  Creating Directory.Build.props..." -ForegroundColor Yellow

    $buildContent = @'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
</Project>
'@
    Set-Content -Path $buildProps -Value $buildContent
    Write-Host "  Created Directory.Build.props" -ForegroundColor Green
}

Write-Host "  Files are in correct location" -ForegroundColor Green

# Step 2: Verify Directory.Build.props content
Write-Host ""
Write-Host "[2/6] Verifying Directory.Build.props..." -ForegroundColor Yellow

$buildContent = Get-Content $buildProps -Raw
if ($buildContent -notmatch "ManagePackageVersionsCentrally") {
    Write-Host "  ERROR: ManagePackageVersionsCentrally not enabled!" -ForegroundColor Red
    exit 1
}

Write-Host "  CPM is enabled" -ForegroundColor Green

# Step 3: Remove Version attributes from .csproj files
Write-Host ""
Write-Host "[3/6] Removing Version attributes from .csproj files..." -ForegroundColor Yellow

$fixedCount = 0
$csprojFiles = Get-ChildItem -Recurse -Filter "*.csproj"

foreach ($file in $csprojFiles) {
    $filePath = $file.FullName
    $relativePath = $filePath.Replace($solutionDir, "").TrimStart('\')
    $content = Get-Content $filePath -Raw

    # Check if file has Version attributes in PackageReference
    if ($content -match 'Version="') {
        Write-Host "  Fixing: $relativePath" -ForegroundColor Cyan

        # Remove Version attributes
        $pattern = '(<PackageReference\s+Include="[^"]+"\s+)Version="[^"]*"\s*'
        $replacement = '$1'
        $fixed = $content -replace $pattern, $replacement

        Set-Content -Path $filePath -Value $fixed -NoNewline
        $fixedCount++
    }
}

Write-Host "  Fixed $fixedCount project files" -ForegroundColor Green

# Step 4: Clear NuGet caches
Write-Host ""
Write-Host "[4/6] Clearing NuGet caches..." -ForegroundColor Yellow

dotnet nuget locals all --clear | Out-Null
Write-Host "  NuGet cache cleared" -ForegroundColor Green

# Step 5: Delete bin/obj folders
Write-Host ""
Write-Host "[5/6] Deleting bin/obj folders..." -ForegroundColor Yellow

Get-ChildItem -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "  Deleted bin/obj folders" -ForegroundColor Green

# Step 6: Restore packages
Write-Host ""
Write-Host "[6/6] Restoring packages..." -ForegroundColor Yellow

$restoreResult = dotnet restore 2>&1
$restoreExitCode = $LASTEXITCODE

if ($restoreExitCode -eq 0) {
    Write-Host "  Restore successful!" -ForegroundColor Green
} else {
    Write-Host "  Restore failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Restore output:" -ForegroundColor Yellow
    $restoreResult | ForEach-Object { Write-Host $_ }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DONE!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($restoreExitCode -eq 0) {
    Write-Host "All issues fixed! Try building now: dotnet build" -ForegroundColor Green
} else {
    Write-Host "Still having issues. Check the error output above." -ForegroundColor Red
}
