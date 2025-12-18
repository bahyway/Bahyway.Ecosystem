# 1. SETUP LOGGING
$LogFile = "$PWD/webview_swap_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 SWAPPING TO CORRECT WEBVIEW PACKAGE..." -ForegroundColor Cyan

# Define Paths
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"
$programFile = "$uiPath/Program.cs"
$controlFile = "$uiPath/Controls/WebGraphControl.cs"

# ---------------------------------------------------------
# 2. UPDATE .CSPROJ (Swap Package)
# ---------------------------------------------------------
Write-Host "📝 Updating Project File..."
# We remove the old package and add 'Avalonia.WebView.Desktop'
$projContent = Get-Content $projFile
$newProjContent = $projContent -replace 'Avalonia.Controls.WebView', 'Avalonia.WebView.Desktop'
Set-Content -Path $projFile -Value $newProjContent
Write-Host "✅ Replaced package reference."

# ---------------------------------------------------------
# 3. UPDATE Program.cs (Register WebView)
# ---------------------------------------------------------
Write-Host "📝 Updating Program.cs to register WebView..."
# We need to add .UseDesktopWebView() to the builder
$programCode = @'
using Avalonia;
using System;
using Avalonia.WebView.Desktop; // Required for the new package

namespace Bahyway.KGEditor.UI
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseDesktopWebView(); // <--- CRITICAL: Registers the engine
    }
}
'@
Set-Content -Path $programFile -Value $programCode -Encoding UTF8

# ---------------------------------------------------------
# 4. UPDATE WebGraphControl.cs (Correct Namespace)
# ---------------------------------------------------------
Write-Host "📝 Updating WebGraphControl.cs..."
$controlCode = @'
using Avalonia.Controls;
using Avalonia.WebView; // Now this namespace will exist!
using System;
using System.IO;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        private WebView _webView;

        public WebGraphControl()
        {
            _webView = new WebView();
            this.Content = _webView;

            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ontoway_engine.html");
            if (File.Exists(htmlPath))
            {
                // Note: Some versions use Url instead of Source
                _webView.Url = new Uri(htmlPath);
            }
        }

        public void RenderGraph(object exportData)
        {
            // Implementation placeholder - we will refine this once it builds
        }
    }
}
'@
Set-Content -Path $controlFile -Value $controlCode -Encoding UTF8

# ---------------------------------------------------------
# 5. CLEAN & BUILD
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "📦 Restoring (Downloading new package)..."
dotnet restore $projFile

Write-Host "🔨 Building..."
dotnet build $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ BUILD SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED. Please check the log above." -ForegroundColor Red
}

Stop-Transcript
