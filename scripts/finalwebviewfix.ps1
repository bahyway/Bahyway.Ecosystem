# 1. START LOGGING
$LogFile = "$PWD/final_webview_fix.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 REMOVING DUPLICATE PACKAGE VERSIONS..." -ForegroundColor Cyan

# Define Paths
$cpmFile = "Directory.Packages.props"
$webFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Controls/WebGraphControl.cs"
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

# ---------------------------------------------------------
# 2. FIX Directory.Packages.props (REMOVE DUPLICATES)
# ---------------------------------------------------------
$content = Get-Content $cpmFile
$newContent = @()
$seenWebView = $false

foreach ($line in $content) {
    if ($line -match "Avalonia.Controls.WebView") {
        if (-not $seenWebView) {
            # Keep the first one, ensure version is 11.0.6
            $newContent += '    <PackageVersion Include="Avalonia.Controls.WebView" Version="11.0.6" />'
            $seenWebView = $true
        }
        # Else: Skip duplicate
    }
    else {
        $newContent += $line
    }
}
Set-Content -Path $cpmFile -Value $newContent
Write-Host "✅ Directory.Packages.props cleaned."

# ---------------------------------------------------------
# 3. FIX WebGraphControl.cs (ENSURE CORRECT NAMESPACE)
# ---------------------------------------------------------
$webCode = @'
using Avalonia.Controls;
using System;
using System.IO;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        // WebView is located in Avalonia.Controls in v11.0.6
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
Write-Host "✅ WebGraphControl.cs updated."

# ---------------------------------------------------------
# 4. CLEAN & BUILD
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "🔨 Building..."
$buildOutput = dotnet build $projFile 2>&1 | Out-String
Write-Host $buildOutput

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED." -ForegroundColor Red
}

Stop-Transcript
