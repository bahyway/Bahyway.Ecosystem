# 1. SETUP LOGGING
$LogFile = "$PWD/text_surgery_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 STARTING TEXT-BASED REPAIR..." -ForegroundColor Cyan

# Define Paths
$rootPath = $PWD
$cpmFile = "$rootPath/Directory.Packages.props"
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

# ---------------------------------------------------------
# 2. SURGERY ON Directory.Packages.props
# ---------------------------------------------------------
Write-Host "📝 Reading Directory.Packages.props..."
$lines = Get-Content $cpmFile

# Filter out ANY existing WebView lines (removes duplicates)
$cleanLines = $lines | Where-Object { $_ -notmatch "Avalonia.Controls.WebView" }

# Convert back to a single string
$content = $cleanLines -join "`r`n"

# Inject the correct line right before the last closing ItemGroup
# We look for the last </ItemGroup> and replace it
$correctLine = '    <PackageVersion Include="Avalonia.Controls.WebView" Version="11.0.6" />'
$regex = "(?s)(.*)(</ItemGroup>)"
# (?s) enables single-line mode so . matches newlines
# We replace the LAST </ItemGroup> we find with our line + </ItemGroup>

if ($content -match $regex) {
    # Replace the last occurrence
    $newContent = $content.Substring(0, $content.LastIndexOf("</ItemGroup>")) + $correctLine + "`r`n  </ItemGroup>" + $content.Substring($content.LastIndexOf("</ItemGroup>") + 12)
    Set-Content -Path $cpmFile -Value $newContent -Encoding UTF8
    Write-Host "✅ Injected clean WebView version."
}
else {
    Write-Error "❌ Could not find </ItemGroup> tag in props file!"
}

# ---------------------------------------------------------
# 3. VERIFY & PRINT FILE CONTENT (DEBUGGING)
# ---------------------------------------------------------
Write-Host "🔎 VERIFYING FILE CONTENT (Last 10 lines):" -ForegroundColor Gray
Get-Content $cpmFile | Select-Object -Last 10

# ---------------------------------------------------------
# 4. BUILD & RUN
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🔨 Building..."
# We stream the build output directly to the console AND log
dotnet build $projFile 2>&1 | Tee-Object -Variable buildOutput

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED." -ForegroundColor Red
}

Stop-Transcript
