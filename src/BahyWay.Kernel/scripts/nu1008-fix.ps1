# 1. SETUP LOGGING
$LogFile = "$PWD/nu1008_fix.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 FIXING NU1008 (CENTRAL PACKAGE MANAGEMENT ERROR)..." -ForegroundColor Cyan

# Define Paths
$rootPath = $PWD
$cpmFile = "$rootPath/Directory.Packages.props"
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

# ---------------------------------------------------------
# 2. UPDATE Directory.Packages.props (Add PackageVersion)
# ---------------------------------------------------------
$cpmContent = Get-Content $cpmFile
if ($cpmContent -notmatch "Avalonia.WebView.Desktop") {
    Write-Host "📝 Adding Version 11.1.0 to Directory.Packages.props..."
    # Add it right before the closing ItemGroup
    $newCpmContent = $cpmContent -replace "</ItemGroup>", "    <PackageVersion Include=""Avalonia.WebView.Desktop"" Version=""11.1.0"" />`r`n  </ItemGroup>"
    Set-Content -Path $cpmFile -Value $newCpmContent -Encoding UTF8
}
else {
    Write-Host "✅ Directory.Packages.props already has the version."
}

# ---------------------------------------------------------
# 3. UPDATE .csproj (Remove Version Attribute)
# ---------------------------------------------------------
Write-Host "📝 Removing explicit version from .csproj..."
$projContent = Get-Content $projFile
# Regex to find PackageReference with Version and strip the Version part
$newProjContent = $projContent -replace 'Include="Avalonia.WebView.Desktop" Version="[^"]+"', 'Include="Avalonia.WebView.Desktop"'
Set-Content -Path $projFile -Value $newProjContent -Encoding UTF8

# ---------------------------------------------------------
# 4. BUILD & RUN
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🚀 Launching..."
dotnet run --project $projFile

Stop-Transcript
