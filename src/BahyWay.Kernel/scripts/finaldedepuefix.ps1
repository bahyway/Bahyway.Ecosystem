# 1. SETUP LOGGING
$LogFile = "$PWD/final_dedupe_fix.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 REMOVING DUPLICATES & FIXING BUILD..." -ForegroundColor Cyan

# Define Paths
$cpmFile = "Directory.Packages.props"
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

# ---------------------------------------------------------
# 2. DEDUPLICATE Directory.Packages.props
# ---------------------------------------------------------
Write-Host "📝 Cleaning Directory.Packages.props..."

# Read file, exclude ANY line that mentions Avalonia.Controls.WebView
$lines = Get-Content $cpmFile
$cleanLines = $lines | Where-Object { $_ -notmatch "Avalonia.Controls.WebView" }

# Reconstruct the file content
$newContent = $cleanLines -join "`r`n"

# Add the SINGLE correct line back right before the closing </ItemGroup>
$newContent = $newContent -replace "</ItemGroup>", "  <PackageVersion Include=""Avalonia.Controls.WebView"" Version=""11.0.6"" />`r`n  </ItemGroup>"

# Save it
Set-Content -Path $cpmFile -Value $newContent -Encoding UTF8
Write-Host "✅ Duplicates removed. Only one WebView version remains."

# ---------------------------------------------------------
# 3. VERIFY CODE (WebGraphControl.cs)
# ---------------------------------------------------------
# Ensure correct namespace is used
$webFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Controls/WebGraphControl.cs"
$webCode = @'
using Avalonia.Controls;
using System;
using System.IO;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        // With the package fixed, this class should now be found
        private WebView _webView;

        public WebGraphControl()
        {
            _webView = new WebView();
            this.Content = _webView;

            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ontoway_engine.html");
            if (File.Exists(htmlPath))
            {
                _webView.Source = new Uri(htmlPath);
            }
        }
    }
}
'@
Set-Content -Path $webFile -Value $webCode -Encoding UTF8

# ---------------------------------------------------------
# 4. NUCLEAR CLEAN & BUILD
# ---------------------------------------------------------
Write-Host "🧹 Nuking obj/bin folders..."
Remove-Item -Path "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/bin" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🔨 Building..."
dotnet build $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ BUILD SUCCESS! Launching Ontoway..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED." -ForegroundColor Red
}

Stop-Transcript
