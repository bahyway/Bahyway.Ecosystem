# ============================================================
# FIX: Remove ManagePackageVersionsCentrally=false
# ============================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FIXING CPM OPT-OUT ISSUE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$files = @(
    "C:\Dev\BahyWay\src\01_Platform\BahyWay.RulesEngine\BahyWay.RulesEngine.csproj",
    "C:\Dev\BahyWay\src\03_Services\Bahyway.KGEditor\Bahyway.KGEditor.UI\Bahyway.KGEditor.UI.csproj"
)

foreach ($file in $files) {
    $projectName = Split-Path $file -Leaf
    Write-Host "`nProcessing: $projectName" -ForegroundColor Yellow

    # Read file
    $content = Get-Content $file -Raw

    # Check if it has the problematic line
    if ($content -match "ManagePackageVersionsCentrally.*false") {
        Write-Host "  Found: <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>" -ForegroundColor Red

        # Remove the line (and the comment before it)
        $fixed = $content -replace '\s*<!\-\-.*CRITICAL.*disables.*central.*\-\->\s*', ''
        $fixed = $fixed -replace '\s*<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>\s*', ''

        # Save
        Set-Content -Path $file -Value $fixed -NoNewline

        Write-Host "  Removed the line! CPM now enabled for this project." -ForegroundColor Green
    } else {
        Write-Host "  Already fixed (no opt-out line found)" -ForegroundColor Green
    }
}

# Delete obj folders for these projects
Write-Host "`nDeleting obj folders..." -ForegroundColor Yellow
Remove-Item "C:\Dev\BahyWay\src\01_Platform\BahyWay.RulesEngine\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\Dev\BahyWay\src\03_Services\Bahyway.KGEditor\Bahyway.KGEditor.UI\obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  Deleted!" -ForegroundColor Green

# Restore
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "RUNNING DOTNET RESTORE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

dotnet restore --force

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n" -NoNewline
    Write-Host "✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓" -ForegroundColor Green
    Write-Host "✓✓✓   SUCCESS!!!   ✓✓✓" -ForegroundColor Green
    Write-Host "✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓✓" -ForegroundColor Green
    Write-Host "`nAll packages restored successfully!" -ForegroundColor Green
    Write-Host "The problem was the opt-out lines in the .csproj files!" -ForegroundColor Yellow
    Write-Host "`nNext: dotnet build" -ForegroundColor Cyan
} else {
    Write-Host "`n❌ Still failed - but this should have fixed it!" -ForegroundColor Red
}
