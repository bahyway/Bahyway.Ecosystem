# ============================================================
# Fix Nested Directory.Build.props Issue
# ============================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FIXING NESTED DIRECTORY.BUILD.PROPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$nestedFile = "C:\Dev\BahyWay\src\01_Platform\ETLWay.Core\Directory.Build.props"

# Step 1: Show current content
Write-Host "`n[1/4] Current content of nested file:" -ForegroundColor Yellow
if (Test-Path $nestedFile) {
    Get-Content $nestedFile
    Write-Host ""
} else {
    Write-Host "  File not found (already deleted?)" -ForegroundColor Yellow
}

# Step 2: Ask user what to do
Write-Host "[2/4] What do you want to do?" -ForegroundColor Yellow
Write-Host "  1. DELETE the nested file (recommended)" -ForegroundColor Cyan
Write-Host "  2. MODIFY it to import parent" -ForegroundColor Cyan
Write-Host "  3. SKIP (keep as is)" -ForegroundColor Cyan
$choice = Read-Host "Enter choice (1/2/3)"

if ($choice -eq "1") {
    # Delete it
    Remove-Item $nestedFile -Force -ErrorAction SilentlyContinue
    Write-Host "  Deleted!" -ForegroundColor Green
}
elseif ($choice -eq "2") {
    # Modify to import parent
    $content = @'
<Project>
  <!-- Import parent Directory.Build.props for CPM -->
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />
</Project>
'@
    Set-Content -Path $nestedFile -Value $content -Encoding UTF8
    Write-Host "  Modified to import parent!" -ForegroundColor Green
}
else {
    Write-Host "  Skipped" -ForegroundColor Yellow
}

# Step 3: Delete obj folders in that tree
Write-Host "`n[3/4] Deleting obj folders under ETLWay.Core..." -ForegroundColor Yellow
Get-ChildItem -Path "C:\Dev\BahyWay\src\01_Platform\ETLWay.Core" -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue |
    ForEach-Object {
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Deleted: $($_.FullName)" -ForegroundColor Cyan
    }

# Step 4: Restore
Write-Host "`n[4/4] Running dotnet restore..." -ForegroundColor Yellow
dotnet restore --force

$exitCode = $LASTEXITCODE

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
if ($exitCode -eq 0) {
    Write-Host "✓✓✓ SUCCESS! ✓✓✓" -ForegroundColor Green
    Write-Host "The nested file was the problem!" -ForegroundColor Green
} else {
    Write-Host "✗✗✗ Still failed ✗✗✗" -ForegroundColor Red
    Write-Host "There might be other nested files..." -ForegroundColor Yellow
}
Write-Host "========================================" -ForegroundColor Cyan
