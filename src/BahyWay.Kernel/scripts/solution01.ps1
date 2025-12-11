# 1. DEFINE PATH
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

Write-Host "🔧 FIXING PROJECT REFERENCE PATH..." -ForegroundColor Cyan

# 2. REWRITE .CSPROJ (WITH CORRECT PATH)
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
    <!-- FIX: Changed from ..\..\..\ to ..\..\ -->
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

# 3. BUILD AND RUN
Write-Host "🚀 LAUNCHING..." -ForegroundColor Green
dotnet run --project $projFile
