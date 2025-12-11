# 1. START LOGGING
$LogFile = "$PWD/build_repair_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 STARTING BASIC REPAIR..." -ForegroundColor Cyan

# 2. DEFINE PATHS
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"

# 3. REWRITE THE PROJECT FILE (.csproj)
# This creates a clean definition of dependencies
Write-Host "📝 Writing clean .csproj file..."
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
    <PackageReference Include="Avalonia" Version="11.0.6" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.6" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.6" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.0.6" />
    <PackageReference Include="Avalonia.Diagnostics" Version="11.0.6" />
    <PackageReference Include="Avalonia.Controls.WebView" Version="11.0.6" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Akka.Remote" Version="1.5.15" />
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

# 4. REWRITE STARTUP FILES (Program.cs & App.axaml)
Write-Host "📝 Writing Startup Files..."

# Program.cs
$programCode = @'
using Avalonia;
using System;

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
                .LogToTrace();
    }
}
'@
Set-Content -Path "$uiPath/Program.cs" -Value $programCode -Encoding UTF8

# App.axaml
$appXamlCode = @'
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="Bahyway.KGEditor.UI.App"
             RequestedThemeVariant="Dark">
    <Application.Styles>
        <FluentTheme />
    </Application.Styles>
</Application>
'@
Set-Content -Path "$uiPath/App.axaml" -Value $appXamlCode -Encoding UTF8

# App.axaml.cs
$appCsCode = @'
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Bahyway.KGEditor.UI.ViewModels;
using Bahyway.KGEditor.UI.Views;

namespace Bahyway.KGEditor.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
'@
Set-Content -Path "$uiPath/App.axaml.cs" -Value $appCsCode -Encoding UTF8

# 5. ATTEMPT BUILD
Write-Host "🔨 Building..."
dotnet build $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ BUILD SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED. Please open 'build_repair_log.txt' to see why." -ForegroundColor Red
}

Stop-Transcript
