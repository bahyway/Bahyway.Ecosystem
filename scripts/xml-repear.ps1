# 1. SETUP LOGGING
$LogFile = "$PWD/xml_repair_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 STARTING XML CONFIGURATION REPAIR..." -ForegroundColor Cyan

# Define Paths
$rootPath = $PWD
$cpmFile = "$rootPath/Directory.Packages.props"
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

# ---------------------------------------------------------
# 2. LOAD & CLEAN XML (Directory.Packages.props)
# ---------------------------------------------------------
try {
    Write-Host "📂 Loading Directory.Packages.props..."
    [xml]$xml = Get-Content $cpmFile

    # Find ALL existing WebView entries (duplicates)
    $nodesToRemove = $xml.SelectNodes("//PackageVersion[@Include='Avalonia.Controls.WebView']")

    if ($nodesToRemove.Count -gt 0) {
        Write-Host "⚠️ Found $($nodesToRemove.Count) conflicting entries for WebView. Removing them..."
        foreach ($node in $nodesToRemove) {
            $node.ParentNode.RemoveChild($node) | Out-Null
        }
    }
    else {
        Write-Host "ℹ️ No existing WebView entries found (Clean slate)."
    }

    # ---------------------------------------------------------
    # 3. ADD SINGLE CORRECT ENTRY
    # ---------------------------------------------------------
    Write-Host "📝 Adding correct PackageVersion..."
    # Get the first ItemGroup to add to
    $itemGroup = $xml.Project.ItemGroup[0]

    $newElement = $xml.CreateElement("PackageVersion", $xml.DocumentElement.NamespaceURI)
    $newElement.SetAttribute("Include", "Avalonia.Controls.WebView")
    $newElement.SetAttribute("Version", "11.0.6")

    $itemGroup.AppendChild($newElement) | Out-Null

    # Save
    $xml.Save($cpmFile)
    Write-Host "✅ XML Saved Successfully."
}
catch {
    Write-Error "❌ XML Manipulation Failed: $_"
    Stop-Transcript
    return
}

# ---------------------------------------------------------
# 4. FORCE RESTORE & BUILD
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "📦 Restoring (This refreshes the lock file)..."
dotnet restore $projFile --force

Write-Host "🔨 Building..."
# We capture output to the log via the Transcript
dotnet build $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ BUILD SUCCESS! Launching Ontoway..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED. Please open '$LogFile' to see the exact error." -ForegroundColor Red
}

Stop-Transcript
