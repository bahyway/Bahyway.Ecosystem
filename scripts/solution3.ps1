# 1. SETUP LOGGING
$LogFile = "$PWD/webview_master_fix.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 FIXING MISSING PACKAGE VERSION..." -ForegroundColor Cyan

# Define Paths
$rootPath = $PWD
$cpmFile = "$rootPath/Directory.Packages.props"
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"
$webFile = "$uiPath/Controls/WebGraphControl.cs"

# ---------------------------------------------------------
# 2. FIX Directory.Packages.props (ADD WEBVIEW VERSION)
# ---------------------------------------------------------
# We append the WebView version if it's missing
$cpmContent = Get-Content $cpmFile
if ($cpmContent -notmatch "Avalonia.Controls.WebView") {
    Write-Host "📝 Adding WebView Version to Directory.Packages.props..."
    # Insert before the closing ItemGroup
    $newContent = $cpmContent -replace "</ItemGroup>", "  <PackageVersion Include=""Avalonia.Controls.WebView"" Version=""11.0.6"" />`n  </ItemGroup>"
    Set-Content -Path $cpmFile -Value $newContent
}
else {
    Write-Host "✅ Directory.Packages.props already contains WebView."
}

# ---------------------------------------------------------
# 3. FIX .CSPROJ (ENSURE REFERENCE EXISTS)
# ---------------------------------------------------------
Write-Host "📝 Updating .csproj..."
$projContent = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" />
    <PackageReference Include="Avalonia.Desktop" />
    <PackageReference Include="Avalonia.Themes.Fluent" />
    <PackageReference Include="Avalonia.Fonts.Inter" />
    <PackageReference Include="Avalonia.Diagnostics" />
    <PackageReference Include="Avalonia.Controls.WebView" />
    <PackageReference Include="CommunityToolkit.Mvvm" />
    <PackageReference Include="Akka.Remote" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Bahyway.SharedKernel\Bahyway.SharedKernel.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="Assets\ontoway_engine.html">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
'@
Set-Content -Path $projFile -Value $projContent -Encoding UTF8

# ---------------------------------------------------------
# 4. FIX WebGraphControl.cs (CORRECT NAMESPACE)
# ---------------------------------------------------------
Write-Host "📝 Updating WebGraphControl.cs..."
$webCode = @'
using Avalonia.Controls;
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

            // Path logic
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
# 5. BUILD & RUN (WITH ERROR CAPTURE)
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean $projFile

Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🔨 Building..."
# Capture Output AND Errors to the log variable
$buildOutput = dotnet build $projFile 2>&1 | Out-String
Write-Host $buildOutput

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED. See output above." -ForegroundColor Red
}

Stop-Transcript
