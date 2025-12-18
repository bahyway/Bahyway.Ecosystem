# 1. SETUP LOGGING
$LogFile = "$PWD/guaranteed_run_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🚑 STARTING EMERGENCY FIX TO LAUNCH APP..." -ForegroundColor Cyan

# Define Paths
$rootPath = $PWD
$cpmFile = "$rootPath/Directory.Packages.props"
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"
$webFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Controls/WebGraphControl.cs"

# ---------------------------------------------------------
# 2. REMOVE BROKEN PACKAGE REFERENCES (XML Surgery)
# ---------------------------------------------------------
Write-Host "🔪 Removing broken WebView references..."

# Clean Directory.Packages.props
[xml]$xmlCpm = Get-Content $cpmFile
$nodesToRemove = $xmlCpm.SelectNodes("//PackageVersion[@Include='Avalonia.Controls.WebView']")
foreach ($node in $nodesToRemove) { $node.ParentNode.RemoveChild($node) | Out-Null }
$xmlCpm.Save($cpmFile)

# Clean .csproj
[xml]$xmlProj = Get-Content $projFile
$nodesToRemove = $xmlProj.SelectNodes("//PackageReference[@Include='Avalonia.Controls.WebView']")
foreach ($node in $nodesToRemove) { $node.ParentNode.RemoveChild($node) | Out-Null }
$xmlProj.Save($projFile)

Write-Host "✅ Dependencies cleaned."

# ---------------------------------------------------------
# 3. REWRITE WebGraphControl.cs (PLACEHOLDER)
# ---------------------------------------------------------
# We replace the broken WebView code with a standard Border/TextBlock
Write-Host "📝 Rewriting WebGraphControl.cs to be compilation-safe..."
$webCode = @'
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        public WebGraphControl()
        {
            // Temporary Placeholder until we install the correct CefNet/WebView2 package
            this.Content = new Border
            {
                Background = Brushes.Black,
                Child = new TextBlock
                {
                    Text = "Web Visualizer (Engine Standby)",
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }
    }
}
'@
Set-Content -Path $webFile -Value $webCode -Encoding UTF8

# ---------------------------------------------------------
# 4. BUILD & RUN
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🚀 LAUNCHING..."
dotnet run --project $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ APP STARTED SUCCESSFULLY!" -ForegroundColor Green
}
else {
    Write-Host "❌ STILL FAILED." -ForegroundColor Red
}

Stop-Transcript
