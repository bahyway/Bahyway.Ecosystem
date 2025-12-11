# 1. SETUP LOGGING
$LogFile = "$PWD/webview_fix_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 STARTING WEBVIEW NAMESPACE FIX..." -ForegroundColor Cyan

# 2. DEFINE PATHS
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"
$webGraphFile = "$uiPath/Controls/WebGraphControl.cs"

# ---------------------------------------------------------
# 3. REWRITE WebGraphControl.cs (Fixing the Namespace Error)
# ---------------------------------------------------------
# Problem: 'using Avalonia.WebView;' does not exist.
# Fix: Removed that line. WebView is in 'Avalonia.Controls'.
Write-Host "📝 Repairing WebGraphControl.cs..."

$webCode = @'
using Avalonia.Controls;
using System;
using System.IO;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        // WebView is part of Avalonia.Controls in standard packages
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
Set-Content -Path $webGraphFile -Value $webCode -Encoding UTF8

# ---------------------------------------------------------
# 4. RESTORE & BUILD
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🔨 Building..."
dotnet build $projFile

# 5. CHECK RESULT
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ BUILD SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED. Check '$LogFile' for details." -ForegroundColor Red
}

Stop-Transcript
