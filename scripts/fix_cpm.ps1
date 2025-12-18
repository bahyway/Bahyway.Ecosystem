# ============================================================
# Fix Central Package Management (CPM) Issues
# ============================================================

$solutionDir = "C:\Dev\BahyWay"
cd $solutionDir

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Fixing Central Package Management" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Fix Directory.Build.props
Write-Host "`n1. Checking Directory.Build.props..." -ForegroundColor Yellow
$buildPropsPath = Join-Path $solutionDir "Directory.Build.props"

if (-not (Test-Path $buildPropsPath)) {
    Write-Host "  Creating Directory.Build.props..." -ForegroundColor Yellow

    $buildPropsContent = @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
</Project>
"@
    Set-Content -Path $buildPropsPath -Value $buildPropsContent
    Write-Host "  ✓ Created!" -ForegroundColor Green
} else {
    $content = Get-Content $buildPropsPath -Raw
    if ($content -notmatch "ManagePackageVersionsCentrally") {
        Write-Host "  ❌ Missing ManagePackageVersionsCentrally!" -ForegroundColor Red
        Write-Host "  Please add this to Directory.Build.props:" -ForegroundColor Yellow
        Write-Host "    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>" -ForegroundColor Cyan
    } else {
        Write-Host "  ✓ Directory.Build.props is correct!" -ForegroundColor Green
    }
}

# Step 2: Remove Version attributes from .csproj files
Write-Host "`n2. Fixing .csproj files..." -ForegroundColor Yellow
$fixedCount = 0

Get-ChildItem -Recurse -Filter "*.csproj" | ForEach-Object {
    $filePath = $_.FullName
    $relativePath = $filePath.Replace($solutionDir, "").TrimStart('\')

    $content = Get-Content $filePath -Raw

    # Check if file has Version attributes
    if ($content -match '<PackageReference\s+Include="[^"]+"\s+Version="[^"]*"') {
        Write-Host "  Fixing: $relativePath" -ForegroundColor Cyan

        # Remove Version="" attributes
        $fixedContent = $content -replace '(<PackageReference\s+Include="[^"]+"\s+)Version="[^"]*"\s*(/?>)', '$1$2'

        Set-Content -Path $filePath -Value $fixedContent -NoNewline
        $fixedCount++
        Write-Host "    ✓ Fixed!" -ForegroundColor Green
    }
}

Write-Host "  Fixed $fixedCount project file(s)" -ForegroundColor Green

# Step 3: Check for missing packages
Write-Host "`n3. Checking for missing packages..." -ForegroundColor Yellow

# Get all packages from .csproj files
$allPackages = @{}
Get-ChildItem -Recurse -Filter "*.csproj" | ForEach-Object {
    $xml = [xml](Get-Content $_.FullName)
    $xml.Project.ItemGroup.PackageReference | ForEach-Object {
        if ($_.Include) {
            $allPackages[$_.Include] = $true
        }
    }
}

# Get packages from Directory.Packages.props
$packagesPropsPath = Join-Path $solutionDir "Directory.Packages.props"
$centralPackages = @{}

if (Test-Path $packagesPropsPath) {
    $xml = [xml](Get-Content $packagesPropsPath)
    $xml.Project.ItemGroup.PackageVersion | ForEach-Object {
        if ($_.Include) {
            $centralPackages[$_.Include] = $_.Version
        }
    }
}

# Find missing packages
$missingPackages = $allPackages.Keys | Where-Object { -not $centralPackages.ContainsKey($_) }

if ($missingPackages) {
    Write-Host "  ❌ Missing packages in Directory.Packages.props:" -ForegroundColor Red
    $missingPackages | ForEach-Object {
        Write-Host "    - $_" -ForegroundColor Red
    }
    Write-Host "`n  Add these to Directory.Packages.props with appropriate versions!" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ All packages are defined in Directory.Packages.props!" -ForegroundColor Green
}

# Step 4: Clean and restore
Write-Host "`n4. Cleaning solution..." -ForegroundColor Yellow
dotnet clean -v quiet

Write-Host "  Deleting bin/obj folders..." -ForegroundColor Yellow
Get-ChildItem -Recurse -Directory -Filter "bin" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem -Recurse -Directory -Filter "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`n5. Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Done! Try building your solution now." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
