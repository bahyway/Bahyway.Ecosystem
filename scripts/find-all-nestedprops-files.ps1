Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FINDING ALL NESTED DIRECTORY FILES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$rootDir = "C:\Dev\BahyWay"

# Find ALL Directory.Build.props files
Write-Host "`nSearching for Directory.Build.props files..." -ForegroundColor Yellow
$buildPropsFiles = Get-ChildItem -Path $rootDir -Recurse -Filter "Directory.Build.props" -ErrorAction SilentlyContinue

Write-Host "Found $($buildPropsFiles.Count) file(s):" -ForegroundColor Cyan
foreach ($file in $buildPropsFiles) {
    if ($file.FullName -eq "$rootDir\Directory.Build.props") {
        Write-Host "  [ROOT] $($file.FullName)" -ForegroundColor Green
    } else {
        Write-Host "  [NESTED - DELETE THIS!] $($file.FullName)" -ForegroundColor Red
    }
}

# Find ALL Directory.Packages.props files
Write-Host "`nSearching for Directory.Packages.props files..." -ForegroundColor Yellow
$packagesPropsFiles = Get-ChildItem -Path $rootDir -Recurse -Filter "Directory.Packages.props" -ErrorAction SilentlyContinue

Write-Host "Found $($packagesPropsFiles.Count) file(s):" -ForegroundColor Cyan
foreach ($file in $packagesPropsFiles) {
    if ($file.FullName -eq "$rootDir\Directory.Packages.props") {
        Write-Host "  [ROOT] $($file.FullName)" -ForegroundColor Green
    } else {
        Write-Host "  [NESTED - DELETE THIS!] $($file.FullName)" -ForegroundColor Red
    }
}

Write-Host "`nDo you want to DELETE all nested files? (y/n)" -ForegroundColor Yellow
$response = Read-Host

if ($response -eq 'y') {
    # Delete nested Directory.Build.props
    $buildPropsFiles | Where-Object { $_.FullName -ne "$rootDir\Directory.Build.props" } | ForEach-Object {
        Write-Host "Deleting: $($_.FullName)" -ForegroundColor Red
        Remove-Item $_.FullName -Force
    }

    # Delete nested Directory.Packages.props
    $packagesPropsFiles | Where-Object { $_.FullName -ne "$rootDir\Directory.Packages.props" } | ForEach-Object {
        Write-Host "Deleting: $($_.FullName)" -ForegroundColor Red
        Remove-Item $_.FullName -Force
    }

    Write-Host "`nAll nested files deleted!" -ForegroundColor Green
} else {
    Write-Host "Skipped" -ForegroundColor Yellow
}
